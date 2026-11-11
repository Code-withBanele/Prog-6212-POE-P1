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
    public async Task<IActionResult> Edit(string id)
    {
        using var client = Api();
        using var response = await client.GetAsync($"api/Events/{Uri.EscapeDataString(id)}");
        if (!response.IsSuccessStatusCode) return NotFound();
        var item = await response.Content.ReadFromJsonAsync<EventListItemViewModel>();
        if (item is null) return NotFound();

        var model = new OrganiserEventViewModel
        {
            EventName = item.EventName, Description = item.Description,
            EventDate = item.EventDate, Location = item.Location,
            DistanceKm = item.DistanceKm, EventType = item.EventType,
            CategoriesId = item.CategoriesId, RouteId = item.RouteId
        };
        await LoadEventOptions(model);
        ViewBag.EventId = id;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, OrganiserEventViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEventOptions(model);
            ViewBag.EventId = id;
            return View(model);
        }
        var payload = new
        {
            EventsId = id, model.EventName, model.Description, model.EventDate,
            model.Location, model.DistanceKm, model.EventType, model.CategoriesId,
            model.RouteId, TeamId = "OPEN"
        };
        using var response = await Api().PutAsJsonAsync($"api/Events/{Uri.EscapeDataString(id)}", payload);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "The event could not be updated. Confirm the selected category and route.");
            await LoadEventOptions(model);
            ViewBag.EventId = id;
            return View(model);
        }
        TempData["SuccessMessage"] = "Event details updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        using var response = await Api().DeleteAsync($"api/Events/{Uri.EscapeDataString(id)}");
        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Event deleted.";
        else
            TempData["ErrorMessage"] = response.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "This event has related enrolments and cannot be deleted."
                : "The event could not be deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Enrolments(string id)
    {
        using var client = Api();
        using var eventResponse = await client.GetAsync($"api/Events/{Uri.EscapeDataString(id)}");
        if (!eventResponse.IsSuccessStatusCode) return NotFound();
        var raceEvent = await eventResponse.Content.ReadFromJsonAsync<EventListItemViewModel>();
        if (raceEvent is null) return NotFound();

        using var rowsResponse = await client.GetAsync($"api/Results/event/{Uri.EscapeDataString(id)}");
        if (!rowsResponse.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Enrolments could not be loaded.";
            return RedirectToAction(nameof(Index));
        }
        var rows = await rowsResponse.Content.ReadFromJsonAsync<List<OrganiserEnrolmentViewModel>>() ?? [];
        return View(new OrganiserEnrolmentsPageViewModel { Event = raceEvent, Enrolments = rows });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(string id, string resultId, DateTime? finishTime, int? finishPosition)
    {
        if (finishPosition is <= 0)
        {
            TempData["ErrorMessage"] = "Finishing position must be a positive number.";
            return RedirectToAction(nameof(Enrolments), new { id });
        }
        var payload = new { ResultsId = resultId, FinishTime = finishTime, FinishPosition = finishPosition };
        using var response = await Api().PutAsJsonAsync($"api/Results/{Uri.EscapeDataString(resultId)}", payload);
        TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
            response.IsSuccessStatusCode ? "Race result saved." : "The result could not be saved.";
        return RedirectToAction(nameof(Enrolments), new { id });
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
