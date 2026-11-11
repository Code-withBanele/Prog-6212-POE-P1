using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Participant")]
public sealed class MyRacesController(RaceDayContext context) : ControllerBase
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
            .Where(x => x.ParticipantsId == participantId)
            .Select(x => new { x.ParticipantsId, x.Name, x.Surname, x.Gender, x.Age })
            .SingleOrDefaultAsync();

        return profile is null ? NotFound() : Ok(profile);
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
