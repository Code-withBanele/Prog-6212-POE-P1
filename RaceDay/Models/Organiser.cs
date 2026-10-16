
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models;

public class Organiser
{
    [Key]
    [MaxLength(50)]
    public string OrganisersId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    // Events managed by this organiser
    public ICollection<Event> Events { get; set; } = new List<Event>();
}

