
using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Organiser|Participant)$",
        ErrorMessage = "Role must be Organiser or Participant.")]
    public string Role { get; set; } = "Participant";

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

  
    [RegularExpression("^[MFO]$",
        ErrorMessage = "Gender must be M, F, or O.")]
    public string Gender { get; set; } = "O";

    [Range(1, 120)]
    public int Age { get; set; }
}

