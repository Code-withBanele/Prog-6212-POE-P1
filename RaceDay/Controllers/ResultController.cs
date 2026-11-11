using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Organiser")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context) =>
        _context = context;

    private async Task<string?> GetCurrentOrganiserId()
    {
        var accountId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(accountId)) return null;
        var account = await _context.UserAccounts.AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserAccountId == accountId);
        return account?.OrganisersId;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Result>>> GetAll()
    {
        var organiserId = await GetCurrentOrganiserId();
        if (organiserId is null) return Forbid();
        return await _context.Results.AsNoTracking()
            .Where(r => r.Event != null && r.Event.OrganisersId == organiserId)
            .ToListAsync();
    }

    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetForEvent(string eventId)
    {
        var organiserId = await GetCurrentOrganiserId();
        if (organiserId is null) return Forbid();

        var ownsEvent = await _context.Events.AnyAsync(e =>
            e.EventsId == eventId && e.OrganisersId == organiserId);
        if (!ownsEvent) return NotFound();

        var rows = await _context.Results.AsNoTracking()
            .Where(r => r.EventsId == eventId)
            .Select(r => new
            {
                r.ResultsId, r.EventsId, r.ParticipantsId, r.CategoriesId,
                r.FinishTime, r.FinishPosition,
                ParticipantName = r.Participant == null ? "Participant" :
                    r.Participant.Name + " " + r.Participant.Surname,
                CategoryName = r.Category == null ? "Category" : r.Category.Name
            }).ToListAsync();
        return Ok(rows);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result>> GetById(string id)
    {
        var item = await _context.Results.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost]
    public async Task<ActionResult<Result>> Create(Result item)
    {
        var organiserId = await GetCurrentOrganiserId();
        if (organiserId is null) return Forbid();
        var ownsEvent = await _context.Events.AnyAsync(e =>
            e.EventsId == item.EventsId && e.OrganisersId == organiserId);
        if (!ownsEvent) return Forbid();

        _context.Results.Add(item);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(
                "The result could not be saved. Check the event, participant and category IDs, and ensure there is no duplicate result.");
        }

        return CreatedAtAction(
            nameof(GetById), new { id = item.ResultsId }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Result item)
    {
        if (id != item.ResultsId) return BadRequest();

        var organiserId = await GetCurrentOrganiserId();
        if (organiserId is null) return Forbid();
        var existing = await _context.Results
            .Include(r => r.Event)
            .SingleOrDefaultAsync(r => r.ResultsId == id);
        if (existing?.Event is null || existing.Event.OrganisersId != organiserId)
            return NotFound();

        // Organisers may record results, but cannot move an enrolment to another
        // event or participant through an update request.
        existing.FinishTime = item.FinishTime;
        existing.FinishPosition = item.FinishPosition;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(
                "The result could not be updated. Check its related record IDs and ensure there is no duplicate result.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var organiserId = await GetCurrentOrganiserId();
        if (organiserId is null) return Forbid();
        var item = await _context.Results.Include(r => r.Event)
            .SingleOrDefaultAsync(r => r.ResultsId == id);
        if (item?.Event is null || item.Event.OrganisersId != organiserId)
            return NotFound();

        _context.Results.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}