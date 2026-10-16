using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs;

public class EnrollmentRequest
{
    [Required]
    [MaxLength(50)]
    public string CategoriesId { get; set; } = string.Empty;
}