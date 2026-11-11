using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public sealed class AccountController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var client = httpClientFactory.CreateClient("RaceDayApi");
        using var response = await client.PostAsJsonAsync("api/Auth/login", new { model.Email, model.Password });
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Email or password was not accepted.");
            return View(model);
        }
        var auth = await response.Content.ReadFromJsonAsync<AuthPayload>();
        if (auth is null || string.IsNullOrWhiteSpace(auth.Token) || string.IsNullOrWhiteSpace(auth.Role))
        {
            ModelState.AddModelError(string.Empty, "The API returned an incomplete sign-in response.");
            return View(model);
        }

        HttpContext.Session.SetString("RaceDay.Jwt", auth.Token);
        HttpContext.Session.SetString("RaceDay.Email", auth.Email ?? model.Email);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, auth.Email ?? model.Email),
            new(ClaimTypes.Email, auth.Email ?? model.Email),
            new(ClaimTypes.Role, auth.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.Role is not ("Participant" or "Organiser"))
        {
            ModelState.AddModelError(nameof(model.Role), "Select a valid account role.");
            return View(model);
        }
        if (model.Role == "Participant" && !model.Age.HasValue)
        {
            ModelState.AddModelError(nameof(model.Age), "Age is required for participant accounts.");
            return View(model);
        }
        var client = httpClientFactory.CreateClient("RaceDayApi");
        using var response = await client.PostAsJsonAsync("api/Auth/register", new { model.Email, model.Password, model.Role, model.Name, model.Surname, model.Gender, model.Age });
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Registration failed. The email may already be registered.");
            return View(model);
        }
        TempData["StatusMessage"] = "Account created. Sign in to continue.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    private sealed class AuthPayload
    {
        public string Token { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
