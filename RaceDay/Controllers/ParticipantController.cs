using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Organiser")]
public class ParticipantsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ParticipantsController(RaceDayContext context) =>
        _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Participant>>> GetAll() =>
        await _context.Participants.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Participant>> GetById(string id)
    {
        var item = await _context.Participants.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost]
    public async Task<ActionResult<Participant>> Create(Participant item)
    {
        _context.Participants.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById), new { id = item.ParticipantsId }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Participant item)
    {
        if (id != item.ParticipantsId) return BadRequest();

        var existing = await _context.Participants.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Name = item.Name;
        existing.Surname = item.Surname;
        existing.Gender = item.Gender;
        existing.Age = item.Age;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var item = await _context.Participants.FindAsync(id);
        if (item is null) return NotFound();

        _context.Participants.Remove(item);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("This participant is still referenced by other records.");
        }

        return NoContent();
    }
}