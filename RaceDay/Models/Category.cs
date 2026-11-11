using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models;

public class Category
{
    [Key, MaxLength(50)]
    public string CategoriesId { get; set; } = Guid.NewGuid().ToString();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    // Legacy relationship retained while existing API clients are supported.
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}
