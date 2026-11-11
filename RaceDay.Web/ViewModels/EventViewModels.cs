namespace RaceDay.Web.ViewModels;

public sealed class EventListItemViewModel
{
    public string EventsId { get; set; } = "";
    public string EventName { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public string CategoriesId { get; set; } = "";
    public List<string> CategoryIds { get; set; } = [];
    public string RouteId { get; set; } = "";
    public string TeamId { get; set; } = "";
    public string OrganisersId { get; set; } = "";
    public string? BannerUrl { get; set; }
}

public sealed class CategoryOptionViewModel
{
    public string CategoriesId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class EventDetailsViewModel
{
    public EventListItemViewModel Event { get; set; } = new();
    public List<CategoryOptionViewModel> Categories { get; set; } = [];
}
