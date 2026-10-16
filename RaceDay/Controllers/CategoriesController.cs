using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public CategoriesController(RaceDayContext context) =>
        _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll() =>
        await _context.Categories.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetById(string id)
    {
        var item = await _context.Categories.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<Category>> Create(Category item)
    {
        _context.Categories.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById), new { id = item.CategoriesId }, item);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(string id, Category item)
    {
        if (id != item.CategoriesId) return BadRequest();

        var existing = await _context.Categories.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Name = item.Name;
        existing.Description = item.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(string id)
    {
        var item = await _context.Categories.FindAsync(id);
        if (item is null) return NotFound();

        _context.Categories.Remove(item);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("This category is still referenced by other records.");
        }

        return NoContent();
    }
}