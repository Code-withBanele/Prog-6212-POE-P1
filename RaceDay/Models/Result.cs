
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models;

public class Result
{
    [Key]
    [MaxLength(50)]
    public string ResultsId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(50)]
    public string EventsId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ParticipantsId { get; set; } = string.Empty;

    // Null until the organiser records the participant's finish time
    public DateTime? FinishTime { get; set; }

    // Null until the organiser records the participant's finishing position
    [Range(1, int.MaxValue)]
    public int? FinishPosition { get; set; }

    [Required]
    [MaxLength(50)]
    public string CategoriesId { get; set; } = string.Empty;

    public Event? Event { get; set; }
    public Participant? Participant { get; set; }
    public Category? Category { get; set; }
}
