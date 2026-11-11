using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models;

/// <summary>
/// A category offered by a specific event. The composite key prevents
/// duplicate category assignments for the same event.
/// </summary>
public class EventCategory
{
    [Required, MaxLength(50)]
    public string EventsId { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string CategoriesId { get; set; } = string.Empty;

    public Event? Event { get; set; }
    public Category? Category { get; set; }
}
