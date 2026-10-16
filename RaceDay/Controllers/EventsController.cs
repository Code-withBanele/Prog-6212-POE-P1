using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EventsController(RaceDayContext context) =>
        _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetAll() =>
        await _context.Events.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetById(string id)
    {
        var item = await _context.Events.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<Event>> Create(Event item)
    {
        _context.Events.Add(item);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(
                "The event could not be saved. Check its category, route and organiser IDs.");
        }

        return CreatedAtAction(
            nameof(GetById), new { id = item.EventsId }, item);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(string id, Event item)
    {
        if (id != item.EventsId) return BadRequest();

        var existing = await _context.Events.FindAsync(id);
        if (existing is null) return NotFound();

        existing.EventName = item.EventName;
        existing.Description = item.Description;
        existing.EventDate = item.EventDate;
        existing.Location = item.Location;
        existing.DistanceKm = item.DistanceKm;
        existing.EventType = item.EventType;
        existing.CategoriesId = item.CategoriesId;
        existing.RouteId = item.RouteId;
        existing.TeamId = item.TeamId;
        existing.OrganisersId = item.OrganisersId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(
                "The event could not be updated. Check its related record IDs.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(string id)
    {
        var item = await _context.Events.FindAsync(id);
        if (item is null) return NotFound();

        _context.Events.Remove(item);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("The event could not be deleted because related records prevent it.");
        }

        return NoContent();
    }
}