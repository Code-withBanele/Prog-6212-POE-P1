using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;
using RaceDay.DTOs;
using RaceDay.Services;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly IBlobStorageService _blobStorage;

    public EventsController(RaceDayContext context, IBlobStorageService blobStorage)
    {
        _context = context;
        _blobStorage = blobStorage;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetAll()
    {
        var events = await _context.Events.AsNoTracking()
            .Include(e => e.EventCategories)
            .ToListAsync();
        foreach (var item in events)
        {
            item.CategoryIds = item.EventCategories.Select(ec => ec.CategoriesId).ToList();
            item.BannerUrl = item.BannerBlobName is null ? null : $"{Request.Scheme}://{Request.Host}/api/Events/{item.EventsId}/banner";
        }
        return events;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetById(string id)
    {
        var item = await _context.Events
            .Include(e => e.EventCategories)
            .SingleOrDefaultAsync(e => e.EventsId == id);
        if (item is null) return NotFound();
        item.CategoryIds = item.EventCategories.Select(ec => ec.CategoriesId).ToList();
        item.BannerUrl = item.BannerBlobName is null ? null : $"{Request.Scheme}://{Request.Host}/api/Events/{item.EventsId}/banner";
        return item;
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<Event>> Create(Event item)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId)) return Unauthorized();

        var account = await _context.UserAccounts
            .SingleOrDefaultAsync(u => u.UserAccountId == accountId);
        if (account?.OrganisersId is null) return Forbid();

        var categoryIds = NormaliseCategoryIds(item);
        if (categoryIds.Count == 0)
            return BadRequest("Select at least one category for this event.");

        var validCategoryCount = await _context.Categories
            .CountAsync(c => categoryIds.Contains(c.CategoriesId));
        if (validCategoryCount != categoryIds.Count)
            return BadRequest("One or more selected categories do not exist.");

        item.CategoriesId = categoryIds[0];
        item.CategoryIds = categoryIds;
        item.OrganisersId = account.OrganisersId;
        item.EventCategories = categoryIds.Select(categoryId => new EventCategory
        {
            EventsId = item.EventsId,
            CategoriesId = categoryId
        }).ToList();

        _context.Events.Add(item);
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            return BadRequest("The event could not be saved. Check its route and team details.");
        }

        return CreatedAtAction(nameof(GetById), new { id = item.EventsId }, item);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(string id, Event item)
    {
        if (id != item.EventsId) return BadRequest();

        var organiserId = await GetCurrentOrganiserId();
        if (organiserId is null) return Forbid();

        var existing = await _context.Events
            .Include(e => e.EventCategories)
            .SingleOrDefaultAsync(e => e.EventsId == id && e.OrganisersId == organiserId);
        if (existing is null) return NotFound();

        var categoryIds = NormaliseCategoryIds(item);
        if (categoryIds.Count == 0)
            return BadRequest("Select at least one category for this event.");

        var validCategoryCount = await _context.Categories
            .CountAsync(c => categoryIds.Contains(c.CategoriesId));
        if (validCategoryCount != categoryIds.Count)
            return BadRequest("One or more selected categories do not exist.");

        existing.EventName = item.EventName;
        existing.Description = item.Description;
        existing.EventDate = item.EventDate;
        existing.Location = item.Location;
        existing.DistanceKm = item.DistanceKm;
        existing.EventType = item.EventType;
        existing.RouteId = item.RouteId;
        existing.TeamId = item.TeamId;
        var categoriesBeingRemoved = existing.EventCategories
            .Where(ec => !categoryIds.Contains(ec.CategoriesId))
            .Select(ec => ec.CategoriesId)
            .ToList();

        var hasEnrolmentsInRemovedCategories = await _context.Results.AnyAsync(r =>
            r.EventsId == id && categoriesBeingRemoved.Contains(r.CategoriesId));
        if (hasEnrolmentsInRemovedCategories)
            return Conflict("A category cannot be removed while participants are enrolled in it.");

        existing.CategoriesId = categoryIds[0];

        var categoriesToRemove = existing.EventCategories
            .Where(ec => categoriesBeingRemoved.Contains(ec.CategoriesId))
            .ToList();
        _context.EventCategories.RemoveRange(categoriesToRemove);

        var existingCategoryIds = existing.EventCategories
            .Where(ec => categoryIds.Contains(ec.CategoriesId))
            .Select(ec => ec.CategoriesId)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var categoryId in categoryIds.Where(id => !existingCategoryIds.Contains(id)))
            existing.EventCategories.Add(new EventCategory
            {
                EventsId = existing.EventsId,
                CategoriesId = categoryId
            });

        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            return BadRequest("The event could not be updated. Check its selected categories and route.");
        }
        return NoContent();
    }

    [HttpPost("{id}/banner")]
    [Authorize(Roles = "Organiser")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadBanner(string id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Choose an image to upload.");
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("The image must be 5 MB or smaller.");

        var contentType = file.ContentType.ToLowerInvariant();
        if (contentType is not ("image/jpeg" or "image/png" or "image/webp"))
            return BadRequest("Only JPG, PNG and WebP images are supported.");

        var organiserId = await GetCurrentOrganiserId();
        if (organiserId is null) return Forbid();

        var item = await _context.Events.SingleOrDefaultAsync(e =>
            e.EventsId == id && e.OrganisersId == organiserId);
        if (item is null) return NotFound();

        try
        {
            await using var stream = file.OpenReadStream();
            item.BannerBlobName = await _blobStorage.UploadAsync(
                stream, file.FileName, contentType, "event-banners");
            await _context.SaveChangesAsync();
            return Ok(new { imageUrl = $"{Request.Scheme}://{Request.Host}/api/Events/{id}/banner" });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }

    [HttpGet("{id}/banner")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBanner(string id)
    {
        var blobName = await _context.Events.AsNoTracking()
            .Where(e => e.EventsId == id)
            .Select(e => e.BannerBlobName)
            .SingleOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(blobName)) return NotFound();

        try
        {
            var image = await _blobStorage.OpenReadAsync(blobName, "event-banners");
            if (image is null) return NotFound();
            Response.Headers.CacheControl = "no-cache";
            return File(image.Value.Content, image.Value.ContentType);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(string id)
    {
        var organiserId = await GetCurrentOrganiserId();
        if (organiserId is null) return Forbid();

        var item = await _context.Events.SingleOrDefaultAsync(e =>
            e.EventsId == id && e.OrganisersId == organiserId);
        if (item is null) return NotFound();

        _context.Events.Remove(item);
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            return Conflict("The event could not be deleted because related records prevent it.");
        }
        return NoContent();
    }

    private static List<string> NormaliseCategoryIds(Event item)
    {
        var ids = item.CategoryIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (ids.Count == 0 && !string.IsNullOrWhiteSpace(item.CategoriesId))
            ids.Add(item.CategoriesId.Trim());
        return ids;
    }

    private async Task<string?> GetCurrentOrganiserId()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId)) return null;
        var account = await _context.UserAccounts.AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserAccountId == accountId);
        return account?.OrganisersId;
    }

    [HttpPost("{eventId}/enrol")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(string eventId, [FromBody] EnrollmentRequest request)
    {
        var userAccountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userAccountId)) return Unauthorized();

        var account = await _context.UserAccounts.AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserAccountId == userAccountId);
        if (account?.ParticipantsId is null) return Forbid();

        var raceEvent = await _context.Events
            .Include(e => e.EventCategories)
            .SingleOrDefaultAsync(e => e.EventsId == eventId);
        if (raceEvent is null) return NotFound("Event not found.");

        var categoryOffered = raceEvent.EventCategories
            .Any(ec => ec.CategoriesId == request.CategoriesId);
        if (!categoryOffered)
            return BadRequest("The selected category is not offered by this event.");

        var alreadyEnrolled = await _context.Results.AnyAsync(r =>
            r.EventsId == eventId && r.ParticipantsId == account.ParticipantsId);
        if (alreadyEnrolled)
            return Conflict("You are already enrolled in this event.");

        var enrollment = new Result
        {
            EventsId = eventId,
            ParticipantsId = account.ParticipantsId,
            CategoriesId = request.CategoriesId,
            FinishTime = null,
            FinishPosition = null
        };
        _context.Results.Add(enrollment);

        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            return Conflict("The enrolment could not be saved. Check whether you are already enrolled.");
        }

        return Created($"/api/Results/{enrollment.ResultsId}", new
        {
            message = "Successfully enrolled in the event.",
            enrollment.ResultsId,
            enrollment.EventsId,
            enrollment.ParticipantsId,
            enrollment.CategoriesId
        });
    }
}
