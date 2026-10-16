using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Organiser")]
public class OrganisersController : ControllerBase
{
    private readonly RaceDayContext _context;

    public OrganisersController(RaceDayContext context) =>
        _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Organiser>>> GetAll() =>
        await _context.Organisers.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Organiser>> GetById(string id)
    {
        var item = await _context.Organisers.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost]
    public async Task<ActionResult<Organiser>> Create(Organiser item)
    {
        _context.Organisers.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById), new { id = item.OrganisersId }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Organiser item)
    {
        if (id != item.OrganisersId) return BadRequest();

        var existing = await _context.Organisers.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Name = item.Name;
        existing.Surname = item.Surname;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var item = await _context.Organisers.FindAsync(id);
        if (item is null) return NotFound();

        _context.Organisers.Remove(item);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("This organiser is still referenced by other records.");
        }

        return NoContent();
    }
}