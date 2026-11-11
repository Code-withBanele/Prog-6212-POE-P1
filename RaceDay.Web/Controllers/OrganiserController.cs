using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public sealed class OrganiserController(IHttpClientFactory clients) : Controller
{
    private HttpClient Api()
    {
        var client = clients.CreateClient("RaceDayApi");
        var token = HttpContext.Session.GetString("RaceDay.Jwt");
        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<IActionResult> Index()
    {
        var client = Api();
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

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new OrganiserEventViewModel();
        if (!await LoadCategories(model)) return RedirectToAction(nameof(Index));
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganiserEventViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategories(model);
            return View(model);
        }

        var payload = new
        {
            EventName = model.EventName,
            Description = model.Description,
            EventDate = model.EventDate,
            Location = model.Location,
            DistanceKm = model.DistanceKm,
            EventType = model.EventType,
            CategoriesId = model.CategoriesId,
            RouteId = model.RouteId,
            TeamId = model.TeamId
        };

        using var response = await Api().PostAsJsonAsync("api/Events", payload);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(detail) ? "The API could not create this event. Check the category, route and team IDs." : detail);
            await LoadCategories(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Your event has been created.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> LoadCategories(OrganiserEventViewModel model)
    {
        var categories = await Api().GetFromJsonAsync<List<CategoryOptionViewModel>>("api/Categories");
        model.Categories = categories ?? [];
        if (model.Categories.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No categories are available. Create a category through the API before creating an event.");
            TempData["ErrorMessage"] = "No event categories are configured yet.";
            return false;
        }
        return true;
    }

    private sealed class ResultSummary
    {
        public string ResultsId { get; set; } = "";
    }
}
