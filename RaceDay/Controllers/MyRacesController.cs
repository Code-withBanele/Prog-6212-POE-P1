using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Services;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Participant")]
public sealed class MyRacesController(RaceDayContext context, IBlobStorageService blobStorage) : ControllerBase
{
    private async Task<string?> GetParticipantId()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId)) return null;

        var account = await context.UserAccounts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserAccountId == accountId);
        return account?.ParticipantsId;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRaces()
    {
        var participantId = await GetParticipantId();
        if (participantId is null) return Forbid();

        var races = await context.Results.AsNoTracking()
            .Where(x => x.ParticipantsId == participantId)
            .Select(x => new
            {
                x.ResultsId,
                x.EventsId,
                EventName = x.Event!.EventName,
                EventDate = x.Event.EventDate,
                Location = x.Event.Location,
                DistanceKm = x.Event.DistanceKm,
                EventType = x.Event.EventType,
                x.CategoriesId,
                CategoryName = x.Category!.Name,
                x.FinishTime,
                x.FinishPosition,
                Status = x.FinishTime == null ? "Enrolled" : "Completed"
            })
            .OrderBy(x => x.EventDate)
            .ToListAsync();

        return Ok(races);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var participantId = await GetParticipantId();
        if (participantId is null) return Forbid();

        var profile = await context.Participants.AsNoTracking()
            .SingleOrDefaultAsync(x => x.ParticipantsId == participantId);
        if (profile is null) return NotFound();

        return Ok(new
        {
            profile.ParticipantsId,
            profile.Name,
            profile.Surname,
            profile.Gender,
            profile.Age,
            ProfilePictureUrl = profile.ProfilePictureBlobName is null
                ? null
                : $"{Request.Scheme}://{Request.Host}/api/MyRaces/profile-picture"
        });
    }

    [HttpPost("profile-picture")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Choose an image to upload.");
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("The image must be 5 MB or smaller.");

        var contentType = file.ContentType.ToLowerInvariant();
        if (contentType is not ("image/jpeg" or "image/png" or "image/webp"))
            return BadRequest("Only JPG, PNG and WebP images are supported.");

        var participantId = await GetParticipantId();
        if (participantId is null) return Forbid();

        var participant = await context.Participants.FindAsync(participantId);
        if (participant is null) return NotFound();

        try
        {
            await using var stream = file.OpenReadStream();
            participant.ProfilePictureBlobName = await blobStorage.UploadAsync(
                stream, file.FileName, contentType, "profile-pictures");
            await context.SaveChangesAsync();
            return Ok(new { imageUrl = $"{Request.Scheme}://{Request.Host}/api/MyRaces/profile-picture" });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }

    [HttpGet("profile-picture/{participantId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProfilePicture(string participantId)
    {
        var blobName = await context.Participants.AsNoTracking()
            .Where(p => p.ParticipantsId == participantId)
            .Select(p => p.ProfilePictureBlobName)
            .SingleOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(blobName)) return NotFound();

        try
        {
            var image = await blobStorage.OpenReadAsync(blobName, "profile-pictures");
            return image is null ? NotFound() : File(image.Value.Content, image.Value.ContentType);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(ParticipantProfileUpdateRequest request)
    {
        var participantId = await GetParticipantId();
        if (participantId is null) return Forbid();

        var profile = await context.Participants.FindAsync(participantId);
        if (profile is null) return NotFound();

        profile.Name = request.Name.Trim();
        profile.Surname = request.Surname.Trim();
        profile.Gender = request.Gender;
        profile.Age = request.Age;
        await context.SaveChangesAsync();

        return NoContent();
    }
}
