using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels;

public sealed class RouteOptionViewModel
{
    public string RouteId { get; set; } = "";
    public string RouteName { get; set; } = "";
    public int Duration { get; set; }
    public string StartLocation { get; set; } = "";
    public string EndLocation { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class CreateRouteViewModel
{
    [Required, StringLength(100)]
    public string RouteName { get; set; } = "";

    [Range(1, 10000)]
    public int Duration { get; set; }

    [Required, StringLength(150)]
    public string StartLocation { get; set; } = "";

    [Required, StringLength(150)]
    public string EndLocation { get; set; } = "";

    [Required, StringLength(255)]
    public string Description { get; set; } = "";
}