namespace RaceDay.Web.ViewModels;

public sealed class OrganiserEnrolmentViewModel
{
    public string ResultsId { get; set; } = "";
    public string EventsId { get; set; } = "";
    public string ParticipantsId { get; set; } = "";
    public string CategoriesId { get; set; } = "";
    public string ParticipantName { get; set; } = "Participant";
    public string CategoryName { get; set; } = "Category";
    public DateTime? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
}

public sealed class OrganiserEnrolmentsPageViewModel
{
    public EventListItemViewModel Event { get; set; } = new();
    public List<OrganiserEnrolmentViewModel> Enrolments { get; set; } = [];
}