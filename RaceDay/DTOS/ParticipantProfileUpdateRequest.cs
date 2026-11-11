using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs;

public sealed class ParticipantProfileUpdateRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required, RegularExpression("^[MFO]$")]
    public string Gender { get; set; } = "O";

    [Range(1, 120)]
    public int Age { get; set; }
}
