
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models;

public class Participant
{
    [Key]
    [MaxLength(50)]
    public string ParticipantsId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[MFO]$",
        ErrorMessage = "Gender must be M, F, or O.")]
    public string Gender { get; set; } = "O";

    [Required]
    [Range(1, 120)]
    public int Age { get; set; }


    public ICollection<Result> Results { get; set; } = new List<Result>();
}

