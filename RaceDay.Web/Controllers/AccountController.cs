using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public sealed class AccountController(
    IHttpClientFactory httpClientFactory,
    ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var client = httpClientFactory.CreateClient("RaceDayApi");
            using var response = await client.PostAsJsonAsync(
                "api/Auth/login", new { model.Email, model.Password });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty,
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Email or password was not accepted."
                        : await ReadApiErrorAsync(response, "Sign-in is temporarily unavailable."));
                return View(model);
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthPayload>();
            if (auth is null || string.IsNullOrWhiteSpace(auth.Token)
                || string.IsNullOrWhiteSpace(auth.Role))
            {
                ModelState.AddModelError(string.Empty,
                    "The API returned an incomplete sign-in response. Please try again.");
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
            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "RaceDay API could not be reached during sign-in.");
            ModelState.AddModelError(string.Empty, ApiConnectionMessage);
            return View(model);
        }
        catch (OperationCanceledException) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            ModelState.AddModelError(string.Empty,
                "The sign-in request timed out. Check that the RaceDay API is running and try again.");
            return View(model);
        }
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
            ModelState.AddModelError(nameof(model.Age),
                "Age is required for participant accounts.");
            return View(model);
        }

        if (model.Role == "Organiser")
            model.Age = null;

        var payload = new
        {
            Email = model.Email.Trim(),
            model.Password,
            model.Role,
            Name = model.Name.Trim(),
            Surname = model.Surname.Trim(),
            model.Gender,
            model.Age
        };

        try
        {
            var client = httpClientFactory.CreateClient("RaceDayApi");
            using var response = await client.PostAsJsonAsync("api/Auth/register", payload);

            if (!response.IsSuccessStatusCode)
            {
                var fallback = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Conflict =>
                        "An account with this email already exists. Sign in instead or use another email.",
                    System.Net.HttpStatusCode.BadRequest =>
                        "Some registration details were rejected. Check the highlighted fields and try again.",
                    _ => "The account could not be created. Please try again."
                };
                ModelState.AddModelError(string.Empty,
                    await ReadApiErrorAsync(response, fallback));
                return View(model);
            }

            TempData["StatusMessage"] =
                "Your RaceDay account has been created. Sign in to continue.";
            return RedirectToAction(nameof(Login));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "RaceDay API could not be reached during registration.");
            ModelState.AddModelError(string.Empty, ApiConnectionMessage);
            return View(model);
        }
        catch (OperationCanceledException) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            ModelState.AddModelError(string.Empty,
                "The registration request timed out. Check that the RaceDay API is running and try again.");
            return View(model);
        }
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    private const string ApiConnectionMessage =
        "RaceDay could not connect to its API. Start the RaceDay API and confirm its URL and HTTPS development certificate, then try again.";

    private static async Task<string> ReadApiErrorAsync(
        HttpResponseMessage response, string fallback)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body)) return fallback;

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            foreach (var property in new[] { "message", "detail", "title" })
            {
                if (root.TryGetProperty(property, out var value)
                    && value.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(value.GetString()))
                    return value.GetString()!;
            }
        }
        catch (JsonException)
        {
            // Do not show raw HTML or stack traces returned by an upstream server.
        }

        return fallback;
    }

    private sealed class AuthPayload
    {
        public string Token { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
