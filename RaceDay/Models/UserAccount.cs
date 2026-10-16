
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models;

public class UserAccount
{
    [Key]
    [MaxLength(50)]
    public string UserAccountId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    // Store a password hash, never the plain-text password
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // Must be "Organiser" or "Participant"
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    // Links the login account to its profile
    [MaxLength(50)]
    public string? OrganisersId { get; set; }

    [MaxLength(50)]
    public string? ParticipantsId { get; set; }

    public Organiser? Organiser { get; set; }
    public Participant? Participant { get; set; }
}

