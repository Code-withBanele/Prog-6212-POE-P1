using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public sealed class OrganiserController(IHttpClientFactory clients) : Controller
{
    public async Task<IActionResult> Index()
    {
        var client = clients.CreateClient("RaceDayApi");
        var token = HttpContext.Session.GetString("RaceDay.Jwt");
        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var eventsResponse = await client.GetAsync("api/Events");
        if (!eventsResponse.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "The event list could not be loaded from the API.";
            return View(new OrganiserDashboardViewModel());
        }

        var events = await eventsResponse.Content.ReadFromJsonAsync<List<EventListItemViewModel>>() ?? [];
        var model = new OrganiserDashboardViewModel
        {
            Events = events.OrderBy(e => e.EventDate).ToList(),
            UpcomingEvents = events.Count(e => e.EventDate >= DateTime.Today),
            CompletedEvents = events.Count(e => e.EventDate < DateTime.Today)
        };

        using var resultsResponse = await client.GetAsync("api/Results");
        if (resultsResponse.IsSuccessStatusCode)
        {
            var results = await resultsResponse.Content.ReadFromJsonAsync<List<ResultSummary>>() ?? [];
            model.TotalEnrolments = results.Count;
        }

        return View(model);
    }

    private sealed class ResultSummary
    {
        public string ResultsId { get; set; } = "";
    }
}
