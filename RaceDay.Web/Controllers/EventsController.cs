using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[Authorize]
public sealed class EventsController(IHttpClientFactory clients) : Controller
{
    private HttpClient Api()
    {
        var client = clients.CreateClient("RaceDayApi");
        var token = HttpContext.Session.GetString("RaceDay.Jwt");
        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Banner(string id)
    {
        using var response = await Api().GetAsync($"api/Events/{Uri.EscapeDataString(id)}/banner");
        if (!response.IsSuccessStatusCode) return NotFound();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        return File(await response.Content.ReadAsByteArrayAsync(), contentType);
    }

    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Index()
    {
        using var response = await Api().GetAsync("api/Events");
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Events could not be loaded. Please try again.";
            return View(new List<EventListItemViewModel>());
        }
        var events = await response.Content.ReadFromJsonAsync<List<EventListItemViewModel>>() ?? [];
        return View(events.OrderBy(e => e.EventDate).ToList());
    }

    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Details(string id)
    {
        using var client = Api();
        using var eventResponse = await client.GetAsync($"api/Events/{Uri.EscapeDataString(id)}");
        if (!eventResponse.IsSuccessStatusCode) return NotFound();
        var item = await eventResponse.Content.ReadFromJsonAsync<EventListItemViewModel>();
        if (item is null) return NotFound();

        item.BannerUrl = Url.Action(nameof(Banner), new { id = item.EventsId });

        using var categoryResponse = await client.GetAsync("api/Categories");
        var categories = categoryResponse.IsSuccessStatusCode
            ? await categoryResponse.Content.ReadFromJsonAsync<List<CategoryOptionViewModel>>() ?? []
            : [];
        return View(new EventDetailsViewModel { Event = item, Categories = categories.Where(c => item.CategoryIds.Contains(c.CategoriesId)).ToList() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(string id, string categoriesId)
    {
        if (string.IsNullOrWhiteSpace(categoriesId))
        {
            TempData["ErrorMessage"] = "Choose an event category before enrolling.";
            return RedirectToAction(nameof(Details), new { id });
        }
        using var response = await Api().PostAsJsonAsync(
            $"api/Events/{Uri.EscapeDataString(id)}/enrol",
            new { CategoriesId = categoriesId });
        if (response.IsSuccessStatusCode)
            TempData["StatusMessage"] = "Your enrolment has been recorded.";
        else
            TempData["ErrorMessage"] = response.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "You may already be enrolled in this event."
                : "Enrolment could not be completed. Please try again.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
