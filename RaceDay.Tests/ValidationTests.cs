using System.ComponentModel.DataAnnotations;
using RaceDay.DTOs;
using RaceDay.Web.ViewModels;
using Xunit;

namespace RaceDay.Tests;

public class ValidationTests
{
    [Fact]
    public void LoginViewModel_AcceptsValidCredentials()
    {
        var model = new LoginViewModel { Email = "runner@example.com", Password = "StrongPass123" };
        Assert.Empty(Validate(model));
    }

    [Theory]
    [InlineData("not-an-email", "StrongPass123")]
    [InlineData("runner@example.com", "short")]
    [InlineData("", "StrongPass123")]
    public void LoginViewModel_RejectsInvalidCredentials(string email, string password)
    {
        var model = new LoginViewModel { Email = email, Password = password };
        Assert.NotEmpty(Validate(model));
    }

    [Fact]
    public void RegisterRequest_AcceptsValidParticipantDetails()
    {
        var model = new RegisterRequest
        {
            Email = "runner@example.com",
            Password = "StrongPass123",
            Role = "Participant",
            Name = "Banele",
            Surname = "Mjayezi",
            Gender = "M",
            Age = 24
        };
        Assert.Empty(Validate(model));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("participant")]
    [InlineData("")]
    public void RegisterRequest_RejectsUnsupportedRoles(string role)
    {
        var model = new RegisterRequest
        {
            Email = "runner@example.com",
            Password = "StrongPass123",
            Role = role,
            Name = "Runner",
            Surname = "Example"
        };
        Assert.Contains(Validate(model), result => result.MemberNames.Contains(nameof(RegisterRequest.Role)));
    }

    [Fact]
    public void RegisterRequest_RejectsInvalidAge()
    {
        var model = new RegisterRequest
        {
            Email = "runner@example.com",
            Password = "StrongPass123",
            Role = "Participant",
            Name = "Runner",
            Surname = "Example",
            Age = 0
        };
        Assert.Contains(Validate(model), result => result.MemberNames.Contains(nameof(RegisterRequest.Age)));
    }

    [Fact]
    public void ParticipantProfile_RejectsInvalidGenderAndAge()
    {
        var model = new ParticipantProfileViewModel
        {
            Name = "Runner",
            Surname = "Example",
            Gender = "X",
            Age = 150
        };
        var errors = Validate(model);
        Assert.Contains(errors, result => result.MemberNames.Contains(nameof(ParticipantProfileViewModel.Gender)));
        Assert.Contains(errors, result => result.MemberNames.Contains(nameof(ParticipantProfileViewModel.Age)));
    }

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}