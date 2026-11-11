using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels;

public sealed class MyRaceViewModel
{
    public string ResultsId { get; set; } = "";
    public string EventsId { get; set; } = "";
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public DateTime? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
    public int EntrantCount { get; set; }
    public string? FinishPositionSummary => FinishPosition.HasValue
        ? $"{FinishPosition.Value}{OrdinalSuffix(FinishPosition.Value)} of {EntrantCount}"
        : null;
    public string Status { get; set; } = "";

    private static string OrdinalSuffix(int value)
    {
        var lastTwo = value % 100;
        if (lastTwo is 11 or 12 or 13) return "th";
        return (value % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
    }
}

public sealed class ParticipantProfileViewModel
{
    public string ParticipantsId { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
    public IFormFile? ProfilePicture { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, MaxLength(100)]
    public string Surname { get; set; } = "";

    [Required, RegularExpression("^[MFO]$")]
    public string Gender { get; set; } = "O";

    [Range(1, 120)]
    public int Age { get; set; }
}
