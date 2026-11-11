using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels;

public sealed class OrganiserEventViewModel
{
    [Required, StringLength(150)]
    public string EventName { get; set; } = "";

    [Required, StringLength(255)]
    public string Description { get; set; } = "";

    [Required, DataType(DataType.DateTime)]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(14);

    [Required, StringLength(150)]
    public string Location { get; set; } = "";

    [Range(0.1, 1000)]
    public decimal DistanceKm { get; set; }

    [Required, RegularExpression("^(Run|Walk|Cycle)$")]
    public string EventType { get; set; } = "Run";

    [Required]
    public string CategoriesId { get; set; } = "";

    [Required, StringLength(50)]
    public string RouteId { get; set; } = "";

    [Required, StringLength(50)]
    public string TeamId { get; set; } = "";

    public List<CategoryOptionViewModel> Categories { get; set; } = [];
}