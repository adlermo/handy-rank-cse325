using System.Net;
using System.Text.RegularExpressions;
using HandyRank.Data;
using HandyRank.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HandyRank.Tests;

public sealed class AuthFlowTests : IClassFixture<HandyRankWebApplicationFactory>
{
    private readonly HandyRankWebApplicationFactory _factory;

    public AuthFlowTests(HandyRankWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterCustomer_CreatesHashedUser_AndSignsIn()
    {
        using var client = CreateClient();
        var token = await GetAntiforgeryTokenAsync(client, "/signup");

        var response = await client.PostAsync("/auth/register", CreateForm(
            token,
            ("email", "customer@example.com"),
            ("password", "Password123!"),
            ("role", "Customer")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/customer/services", response.Headers.Location?.OriginalString);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(user => user.Email == "customer@example.com");

        Assert.Equal(UserRole.Customer, user.Role);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.NotEmpty(user.PasswordHash);

        var protectedPage = await client.GetAsync("/customer/services");
        Assert.Equal(HttpStatusCode.OK, protectedPage.StatusCode);
    }

    [Fact]
    public async Task RegisterDuplicateEmail_RedirectsWithError()
    {
        using var client = CreateClient();

        var firstToken = await GetAntiforgeryTokenAsync(client, "/signup");
        await client.PostAsync("/auth/register", CreateForm(
            firstToken,
            ("email", "duplicate@example.com"),
            ("password", "Password123!"),
            ("role", "Customer")));

        using var secondClient = CreateClient();
        var secondToken = await GetAntiforgeryTokenAsync(secondClient, "/signup");
        var response = await secondClient.PostAsync("/auth/register", CreateForm(
            secondToken,
            ("email", "duplicate@example.com"),
            ("password", "Password123!"),
            ("role", "Handyman")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/signup?error=email-exists", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task LoginHandyman_WithValidPassword_SignsInAndRedirects()
    {
        await SeedUserAsync("pro@example.com", "Password123!", UserRole.Handyman);

        using var client = CreateClient();
        var token = await GetAntiforgeryTokenAsync(client, "/login");

        var response = await client.PostAsync("/auth/login", CreateForm(
            token,
            ("email", "pro@example.com"),
            ("password", "Password123!")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/professional/services", response.Headers.Location?.OriginalString);

        var protectedPage = await client.GetAsync("/professional/services");
        Assert.Equal(HttpStatusCode.OK, protectedPage.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_RedirectsWithInvalidError()
    {
        await SeedUserAsync("wrong-password@example.com", "Password123!", UserRole.Customer);

        using var client = CreateClient();
        var token = await GetAntiforgeryTokenAsync(client, "/login");

        var response = await client.PostAsync("/auth/login", CreateForm(
            token,
            ("email", "wrong-password@example.com"),
            ("password", "bad-password")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login?error=invalid", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Customer_CannotAccessHandymanPage()
    {
        using var client = CreateClient();
        var token = await GetAntiforgeryTokenAsync(client, "/signup");

        await client.PostAsync("/auth/register", CreateForm(
            token,
            ("email", "customer-role@example.com"),
            ("password", "Password123!"),
            ("role", "Customer")));

        var response = await client.GetAsync("/professional/services");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/access-denied", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Logout_ClearsAuthenticationCookie()
    {
        using var client = CreateClient();
        var signupToken = await GetAntiforgeryTokenAsync(client, "/signup");

        await client.PostAsync("/auth/register", CreateForm(
            signupToken,
            ("email", "logout@example.com"),
            ("password", "Password123!"),
            ("role", "Customer")));

        var logoutToken = await GetAntiforgeryTokenAsync(client, "/");
        var response = await client.PostAsync("/auth/logout", CreateForm(logoutToken));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login?message=logged-out", response.Headers.Location?.OriginalString);

        var protectedPage = await client.GetAsync("/customer/services");

        Assert.Equal(HttpStatusCode.Redirect, protectedPage.StatusCode);
        Assert.Equal("/login", protectedPage.Headers.Location?.AbsolutePath);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private async Task SeedUserAsync(string email, string password, UserRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(user => user.Email == normalizedEmail))
        {
            return;
        }

        var user = new User
        {
            Email = normalizedEmail,
            Role = role
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
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
}
