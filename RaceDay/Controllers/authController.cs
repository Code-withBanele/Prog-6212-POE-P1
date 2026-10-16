using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<UserAccount> _passwordHasher = new();

    public AuthController(
        RaceDayContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // POST: api/Auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _context.UserAccounts.AnyAsync(u => u.Email == email))
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        if (request.Role == "Participant" &&
            (!request.Age.HasValue ||
             request.Age < 1 ||
             request.Age > 120))
        {
            return BadRequest(new
            {
                message = "Participants must provide an age between 1 and 120."
            });
        }

        var user = new UserAccount
        {
            Email = email,
            Role = request.Role
        };

        if (request.Role == "Organiser")
        {
            user.Organiser = new Organiser
            {
                Name = request.Name.Trim(),
                Surname = request.Surname.Trim()
            };
        }
        else
        {
            user.Participant = new Participant
            {
                Name = request.Name.Trim(),
                Surname = request.Surname.Trim(),
                Gender = request.Gender,
                Age = request.Age!.Value
            };
        }

        user.PasswordHash = _passwordHasher.HashPassword(
            user, request.Password);

        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(201, new
        {
            message = "Registration successful. You can now log in.",
            email = user.Email,
            role = user.Role
        });
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.UserAccounts
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var verification = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException(
                "JWT secret is missing.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is missing.");

        var expiryMinutes = int.TryParse(
            _configuration["Jwt:ExpiryMinutes"],
            out var minutes)
                ? minutes
                : 60;

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserAccountId),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Ok(new AuthResponse
        {
            Token = tokenString,
            ExpiresAt = expiresAt,
            Email = user.Email,
            Role = user.Role
        });
    }
}