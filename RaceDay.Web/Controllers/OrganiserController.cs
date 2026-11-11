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
        await LoadEventOptions(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganiserEventViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEventOptions(model);
            return View(model);
        }

        var payload = new
        {
            model.EventName,
            model.Description,
            model.EventDate,
            model.Location,
            model.DistanceKm,
            model.EventType,
            model.CategoriesId,
            model.RouteId,
            // The API has no Team entity/table or team-management endpoint.
            // Use the explicit unassigned group value rather than asking users for a fake ID.
            TeamId = "OPEN"
        };

        using var response = await Api().PostAsJsonAsync("api/Events", payload);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(detail) ? "The API could not create this event. Check the selected category and route." : detail);
            await LoadEventOptions(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Your event has been created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CreateRoute() => View(new CreateRouteViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRoute(CreateRouteViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        using var response = await Api().PostAsJsonAsync("api/Routes", model);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(detail) ? "The API could not save this route." : detail);
            return View(model);
        }

        TempData["SuccessMessage"] = "Route created. You can now select it when creating an event.";
        return RedirectToAction(nameof(Create));
    }

    private async Task LoadEventOptions(OrganiserEventViewModel model)
    {
        using var client = Api();
        using var categoriesResponse = await client.GetAsync("api/Categories");
        if (categoriesResponse.IsSuccessStatusCode)
            model.Categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryOptionViewModel>>() ?? [];

        using var routesResponse = await client.GetAsync("api/Routes");
        if (routesResponse.IsSuccessStatusCode)
            model.Routes = await routesResponse.Content.ReadFromJsonAsync<List<RouteOptionViewModel>>() ?? [];

        if (model.Categories.Count == 0)
            ModelState.AddModelError(string.Empty, "No categories are configured. Add a category through the API before creating an event.");
        if (model.Routes.Count == 0)
            ModelState.AddModelError(string.Empty, "No routes exist yet. Create a route first using the button below.");
    }

    private sealed class ResultSummary
    {
        public string ResultsId { get; set; } = "";
    }
}
