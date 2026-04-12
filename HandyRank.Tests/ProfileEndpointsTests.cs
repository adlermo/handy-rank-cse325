using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using HandyRank.Data;
using HandyRank.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HandyRank.Tests;

public sealed class ProfileEndpointsTests : IClassFixture<HandyRankWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HandyRankWebApplicationFactory _factory;

    public ProfileEndpointsTests(HandyRankWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Customer_CanGetAndUpdateBaseProfile()
    {
        using var client = CreateClient();
        await RegisterAsync(client, "profile-customer@example.com", UserRole.Customer);

        var getResponse = await client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var initialProfile = await ReadProfileAsync(getResponse);
        Assert.Equal("Customer", initialProfile.Role);
        Assert.Null(initialProfile.HandymanProfile);

        var updateResponse = await client.PutAsJsonAsync("/api/profile", new
        {
            Name = "Casey Customer",
            Bio = "Homeowner looking for dependable help.",
            Location = "Rexburg, ID"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedProfile = await ReadProfileAsync(updateResponse);
        Assert.Equal("Casey Customer", updatedProfile.Name);
        Assert.Equal("Homeowner looking for dependable help.", updatedProfile.Bio);
        Assert.Equal("Rexburg, ID", updatedProfile.Location);
        Assert.Null(updatedProfile.HandymanProfile);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(user => user.Email == "profile-customer@example.com");

        Assert.Equal("Casey Customer", user.Name);
        Assert.Equal("Homeowner looking for dependable help.", user.Bio);
        Assert.Equal("Rexburg, ID", user.Location);
        Assert.False(await db.HandymanProfiles.AnyAsync(profile => profile.UserId == user.Id));
    }

    [Fact]
    public async Task Handyman_CanUpdateBaseAndProfessionalProfile()
    {
        using var client = CreateClient();
        await RegisterAsync(client, "profile-pro@example.com", UserRole.Handyman);

        var updateResponse = await client.PutAsJsonAsync("/api/profile", new
        {
            Name = "Pat Pro",
            Bio = "Licensed plumber with weekend availability.",
            Location = "Idaho Falls, ID",
            Skills = "Plumbing, fixtures, water heaters"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedProfile = await ReadProfileAsync(updateResponse);
        Assert.Equal("Handyman", updatedProfile.Role);
        Assert.Equal("Pat Pro", updatedProfile.Name);
        Assert.Equal("Licensed plumber with weekend availability.", updatedProfile.Bio);
        Assert.Equal("Idaho Falls, ID", updatedProfile.Location);
        Assert.NotNull(updatedProfile.HandymanProfile);
        Assert.Equal("Plumbing, fixtures, water heaters", updatedProfile.HandymanProfile.Skills);
        Assert.Equal(0, updatedProfile.HandymanProfile.XP);
        Assert.Equal("New", updatedProfile.HandymanProfile.Rank);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users
            .Include(user => user.HandymanProfile)
            .SingleAsync(user => user.Email == "profile-pro@example.com");

        Assert.Equal("Pat Pro", user.Name);
        Assert.Equal("Licensed plumber with weekend availability.", user.Bio);
        Assert.Equal("Idaho Falls, ID", user.Location);
        Assert.NotNull(user.HandymanProfile);
        Assert.Equal("Plumbing, fixtures, water heaters", user.HandymanProfile.Skills);
    }

    [Fact]
    public async Task Customer_UpdateProfile_WithMissingRequiredFields_ReturnsBadRequest()
    {
        using var client = CreateClient();
        await RegisterAsync(client, "profile-validation@example.com", UserRole.Customer);

        var response = await client.PutAsJsonAsync("/api/profile", new
        {
            Name = "",
            Bio = "Valid bio",
            Location = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handyman_UpdateProfile_WithMissingSkills_ReturnsBadRequest()
    {
        using var client = CreateClient();
        await RegisterAsync(client, "profile-skills-validation@example.com", UserRole.Handyman);

        var response = await client.PutAsJsonAsync("/api/profile", new
        {
            Name = "Pat Pro",
            Bio = "Valid bio",
            Location = "Rexburg, ID",
            Skills = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private static async Task RegisterAsync(HttpClient client, string email, UserRole role)
    {
        var token = await GetAntiforgeryTokenAsync(client, "/signup");

        var response = await client.PostAsync("/auth/register", CreateForm(
            token,
            ("email", email),
            ("password", "Password123!"),
            ("role", role.ToString())));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var expectedRedirect = role == UserRole.Handyman
            ? "/professional/servicos-disponiveis"
            : "/customer/meus-servicos";

        Assert.Equal(expectedRedirect, response.Headers.Location?.OriginalString);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");

        Assert.True(match.Success, "Could not find antiforgery token in the page.");

        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static FormUrlEncodedContent CreateForm(
        string antiforgeryToken,
        params (string Name, string Value)[] fields)
    {
        var values = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiforgeryToken
        };

        foreach (var field in fields)
        {
            values[field.Name] = field.Value;
        }

        return new FormUrlEncodedContent(values);
    }

    private static async Task<ProfileResponse> ReadProfileAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<ProfileResponse>(json, JsonOptions);

        Assert.NotNull(profile);
        return profile;
    }

    private sealed record ProfileResponse(
        int Id,
        string Email,
        string Role,
        string Name,
        string Bio,
        string Location,
        HandymanProfileResponse? HandymanProfile);

    private sealed record HandymanProfileResponse(
        string Skills,
        int XP,
        string Rank);
}
