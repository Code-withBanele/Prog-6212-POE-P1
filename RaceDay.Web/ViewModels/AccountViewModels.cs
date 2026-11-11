using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels;

public sealed class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterViewModel
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required, RegularExpression("^(Organiser|Participant)$")]
    public string Role { get; set; } = "Participant";

    [RegularExpression("^[MFO]$")]
    public string Gender { get; set; } = "O";

    [Range(1, 120)]
    public int? Age { get; set; }
}
