using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceRoute = RaceDay.Models.Route;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoutesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public RoutesController(RaceDayContext context) =>
        _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RaceRoute>>> GetAll() =>
        await _context.Routes.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<RaceRoute>> GetById(string id)
    {
        var item = await _context.Routes.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<RaceRoute>> Create(RaceRoute item)
    {
        _context.Routes.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById), new { id = item.RouteId }, item);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(string id, RaceRoute item)
    {
        if (id != item.RouteId) return BadRequest();

        var existing = await _context.Routes.FindAsync(id);
        if (existing is null) return NotFound();

        existing.RouteName = item.RouteName;
        existing.Duration = item.Duration;
        existing.StartLocation = item.StartLocation;
        existing.EndLocation = item.EndLocation;
        existing.Description = item.Description;
        

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(string id)
    {
        var item = await _context.Routes.FindAsync(id);
        if (item is null) return NotFound();

        _context.Routes.Remove(item);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("This route is still referenced by other records.");
        }

        return NoContent();
    }
}