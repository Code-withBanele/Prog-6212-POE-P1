using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Participant")]
public sealed class ParticipantController(IHttpClientFactory clients) : Controller
{
    private HttpClient Api()
    {
        var client = clients.CreateClient("RaceDayApi");
        var token = HttpContext.Session.GetString("RaceDay.Jwt");
        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<IActionResult> MyRaces()
    {
        using var response = await Api().GetAsync("api/MyRaces");
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Your race history could not be loaded.";
            return View(new List<MyRaceViewModel>());
        }
        var races = await response.Content.ReadFromJsonAsync<List<MyRaceViewModel>>() ?? [];
        return View(races);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        using var response = await Api().GetAsync("api/MyRaces/profile");
        if (!response.IsSuccessStatusCode) return View(new ParticipantProfileViewModel());
        var profile = await response.Content.ReadFromJsonAsync<ParticipantProfileViewModel>();
        return View(profile ?? new ParticipantProfileViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ParticipantProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        using var response = await Api().PutAsJsonAsync("api/MyRaces/profile", new
        {
            model.Name, model.Surname, model.Gender, model.Age
        });
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Your profile could not be updated.");
            return View(model);
        }

        if (model.ProfilePicture is not null && !await UploadProfilePictureAsync(model.ProfilePicture))
        {
            TempData["ErrorMessage"] = "Your profile was saved, but the profile picture upload failed. Check Azure Blob Storage configuration and try again.";
            return RedirectToAction(nameof(Profile));
        }

        TempData["StatusMessage"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Profile));
    }

    private async Task<bool> UploadProfilePictureAsync(IFormFile file)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        using var response = await Api().PostAsync("api/MyRaces/profile-picture", content);
        return response.IsSuccessStatusCode;
    }
}
