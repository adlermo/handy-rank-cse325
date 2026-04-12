using System.Security.Claims;
using HandyRank.Data;
using HandyRank.Models;
using Microsoft.EntityFrameworkCore;

namespace HandyRank.Endpoints;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var profile = app.MapGroup("/api/profile")
            .RequireAuthorization();

        profile.MapGet("/me", GetProfileAsync);
        profile.MapPut(string.Empty, UpdateProfileAsync);

        return app;
    }

    private static async Task<IResult> GetProfileAsync(ClaimsPrincipal principal, AppDbContext db)
    {
        var user = await GetCurrentUserAsync(principal, db);

        return user is null
            ? Results.StatusCode(StatusCodes.Status401Unauthorized)
            : Results.Ok(ToResponse(user));
    }

    private static async Task<IResult> UpdateProfileAsync(
        ClaimsPrincipal principal,
        AppDbContext db,
        ProfileUpdateRequest request)
    {
        var user = await GetCurrentUserAsync(principal, db);

        if (user is null)
        {
            return Results.StatusCode(StatusCodes.Status401Unauthorized);
        }

        var errors = Validate(request, user.Role);

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        user.Name = request.Name!.Trim();
        user.Bio = request.Bio!.Trim();
        user.Location = request.Location!.Trim();

        if (user.Role == UserRole.Handyman)
        {
            if (user.HandymanProfile is null)
            {
                user.HandymanProfile = new HandymanProfile
                {
                    UserId = user.Id
                };

                db.HandymanProfiles.Add(user.HandymanProfile);
            }

            user.HandymanProfile.Skills = request.Skills!.Trim();
        }

        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(user));
    }

    private static async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal, AppDbContext db)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return await db.Users
            .Include(user => user.HandymanProfile)
            .SingleOrDefaultAsync(user => user.Id == userId);
    }

    private static Dictionary<string, string[]> Validate(ProfileUpdateRequest request, UserRole role)
    {
        var errors = new Dictionary<string, string[]>();

        AddRequiredError(errors, nameof(request.Name), request.Name);
        AddRequiredError(errors, nameof(request.Bio), request.Bio);
        AddRequiredError(errors, nameof(request.Location), request.Location);

        AddMaxLengthError(errors, nameof(request.Name), request.Name, User.MaxNameLength);
        AddMaxLengthError(errors, nameof(request.Bio), request.Bio, User.MaxBioLength);
        AddMaxLengthError(errors, nameof(request.Location), request.Location, User.MaxLocationLength);

        if (role == UserRole.Handyman)
        {
            AddRequiredError(errors, nameof(request.Skills), request.Skills);
            AddMaxLengthError(errors, nameof(request.Skills), request.Skills, HandymanProfile.MaxSkillsLength);
        }

        return errors;
    }

    private static void AddRequiredError(
        Dictionary<string, string[]> errors,
        string field,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = [$"{field} is required."];
        }
    }

    private static void AddMaxLengthError(
        Dictionary<string, string[]> errors,
        string field,
        string? value,
        int maxLength)
    {
        if (value?.Length > maxLength)
        {
            errors[field] = [$"{field} must be {maxLength} characters or fewer."];
        }
    }

    private static object ToResponse(User user)
    {
        var baseProfile = new
        {
            user.Id,
            user.Email,
            Role = user.Role.ToString(),
            user.Name,
            user.Bio,
            user.Location
        };

        if (user.Role != UserRole.Handyman)
        {
            return baseProfile;
        }

        return new
        {
            user.Id,
            user.Email,
            Role = user.Role.ToString(),
            user.Name,
            user.Bio,
            user.Location,
            HandymanProfile = ToHandymanProfileResponse(user.HandymanProfile)
        };
    }

    private static HandymanProfileResponse ToHandymanProfileResponse(HandymanProfile? profile)
    {
        return profile is null
            ? new HandymanProfileResponse(string.Empty, 0, "New")
            : new HandymanProfileResponse(profile.Skills, profile.XP, profile.Rank);
    }

    private sealed record ProfileUpdateRequest(
        string? Name,
        string? Bio,
        string? Location,
        string? Skills);

    private sealed record HandymanProfileResponse(
        string Skills,
        int XP,
        string Rank);
}
