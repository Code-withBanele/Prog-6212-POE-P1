using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models;

public class Event
{
    [Key, MaxLength(50)]
    public string EventsId { get; set; } = Guid.NewGuid().ToString();

    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public decimal DistanceKm { get; set; }

    [Required, RegularExpression("^(Run|Walk|Cycle)$"), MaxLength(10)]
    public string EventType { get; set; } = "Run";

    // Kept temporarily for compatibility with existing records and clients.
    // CategoriesId always mirrors the first category in CategoryIds.
    [Required, MaxLength(50)]
    public string CategoriesId { get; set; } = string.Empty;

    [NotMapped]
    public List<string> CategoryIds { get; set; } = [];

    [Required, MaxLength(50)]
    public string RouteId { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string TeamId { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string OrganisersId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? BannerBlobName { get; set; }

    [NotMapped]
    public string? BannerUrl { get; set; }

    public Category? Category { get; set; }
    public Route? Route { get; set; }
    public Organiser? Organiser { get; set; }
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    public ICollection<Result> Results { get; set; } = new List<Result>();
}
