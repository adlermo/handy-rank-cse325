using System.Security.Claims;
using System.Net.Mail;
using HandyRank.Data;
using HandyRank.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HandyRank.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");

        auth.MapPost("/register", RegisterAsync)
            .AllowAnonymous();

        auth.MapPost("/login", LoginAsync)
            .AllowAnonymous();

        auth.MapPost("/logout", LogoutAsync)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        HttpContext httpContext,
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IAntiforgery antiforgery)
    {
        var form = await ReadValidatedFormAsync(httpContext, antiforgery);

        if (form is null)
        {
            return Results.Redirect("/signup?error=invalid-form");
        }

        var email = NormalizeEmail(GetFormValue(form, "email"));
        var password = GetFormValue(form, "password");
        var roleValue = GetFormValue(form, "role");
        var returnUrl = GetFormValue(form, "returnUrl");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return RedirectWithError("/signup", "missing", returnUrl);
        }

        if (!IsValidEmail(email))
        {
            return RedirectWithError("/signup", "invalid-email", returnUrl);
        }

        if (password.Length < 8)
        {
            return RedirectWithError("/signup", "weak-password", returnUrl);
        }

        if (!Enum.TryParse<UserRole>(roleValue, ignoreCase: true, out var role)
            || !Enum.IsDefined(role))
        {
            return RedirectWithError("/signup", "invalid-role", returnUrl);
        }

        var emailExists = await db.Users.AnyAsync(user => user.Email == email);

        if (emailExists)
        {
            return RedirectWithError("/signup", "email-exists", returnUrl);
        }

        var user = new User
        {
            Email = email,
            Role = role
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return RedirectWithError("/signup", "email-exists", returnUrl);
        }

        await SignInAsync(httpContext, user);

        return Results.Redirect(GetSafeReturnUrl(returnUrl, user.Role));
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IAntiforgery antiforgery)
    {
        var form = await ReadValidatedFormAsync(httpContext, antiforgery);

        if (form is null)
        {
            return Results.Redirect("/login?error=invalid-form");
        }

        var email = NormalizeEmail(GetFormValue(form, "email"));
        var password = GetFormValue(form, "password");
        var returnUrl = GetFormValue(form, "returnUrl");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return RedirectWithError("/login", "missing", returnUrl);
        }

        if (!IsValidEmail(email))
        {
            return RedirectWithError("/login", "invalid", returnUrl);
        }

        var user = await db.Users.SingleOrDefaultAsync(user => user.Email == email);

        if (user is null)
        {
            return RedirectWithError("/login", "invalid", returnUrl);
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return RedirectWithError("/login", "invalid", returnUrl);
        }

        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            await db.SaveChangesAsync();
        }

        await SignInAsync(httpContext, user);

        return Results.Redirect(GetSafeReturnUrl(returnUrl, user.Role));
    }

    private static async Task<IResult> LogoutAsync(HttpContext httpContext, IAntiforgery antiforgery)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(httpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.Redirect("/login?error=invalid-form");
        }

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/login?message=logged-out");
    }

    private static async Task<IFormCollection?> ReadValidatedFormAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(httpContext);
            return await httpContext.Request.ReadFormAsync();
        }
        catch (AntiforgeryValidationException)
        {
            return null;
        }
    }

    private static async Task SignInAsync(HttpContext httpContext, User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        return email.Length <= 256 && MailAddress.TryCreate(email, out _);
    }

    private static string GetFormValue(IFormCollection form, string key)
    {
        return form.TryGetValue(key, out var value)
            ? value.ToString()
            : string.Empty;
    }

    private static IResult RedirectWithError(string page, string error, string returnUrl)
    {
        var url = $"{page}?error={Uri.EscapeDataString(error)}";

        if (IsSafeLocalUrl(returnUrl))
        {
            url += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        return Results.Redirect(url);
    }

    private static string GetSafeReturnUrl(string returnUrl, UserRole role)
    {
        if (IsSafeLocalUrl(returnUrl))
        {
            return returnUrl;
        }

        return role == UserRole.Handyman
            ? "/professional/servicos-disponiveis"
            : "/customer/meus-servicos";
    }

    private static bool IsSafeLocalUrl(string url)
    {
        return !string.IsNullOrWhiteSpace(url)
            && url.StartsWith('/')
            && !url.StartsWith("//")
            && !url.Contains("://", StringComparison.Ordinal);
    }
}
