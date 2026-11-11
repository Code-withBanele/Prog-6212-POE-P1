namespace RaceDay.Web.ViewModels;

public sealed class OrganiserDashboardViewModel
{
    public List<EventListItemViewModel> Events { get; set; } = [];
    public int TotalEnrolments { get; set; }
    public int UpcomingEvents { get; set; }
    public int CompletedEvents { get; set; }
}
