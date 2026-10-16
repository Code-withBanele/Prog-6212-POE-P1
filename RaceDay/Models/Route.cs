
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models;

public class Route
{
    [Key]
    [MaxLength(50)]
    public string RouteId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    public string RouteName { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int Duration { get; set; } // Duration in minutes

    [Required]
    [MaxLength(150)]
    public string StartLocation { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string EndLocation { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;


    public ICollection<Event> Events { get; set; } = new List<Event>();
}
