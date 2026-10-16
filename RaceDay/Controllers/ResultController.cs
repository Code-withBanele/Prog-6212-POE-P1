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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Result>>> GetAll() =>
        await _context.Results.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Result>> GetById(string id)
    {
        var item = await _context.Results.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost]
    public async Task<ActionResult<Result>> Create(Result item)
    {
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

        var existing = await _context.Results.FindAsync(id);
        if (existing is null) return NotFound();

        existing.EventsId = item.EventsId;
        existing.ParticipantsId = item.ParticipantsId;
        existing.FinishTime = item.FinishTime;
        existing.FinishPosition = item.FinishPosition;
        existing.CategoriesId = item.CategoriesId;

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
        var item = await _context.Results.FindAsync(id);
        if (item is null) return NotFound();

        _context.Results.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}