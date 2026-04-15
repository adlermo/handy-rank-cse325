using System.Security.Claims;
using HandyRank.Data;
using HandyRank.Domain.Enums;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

public class ReviewService
{
    private readonly AppDbContext _db;
    private readonly AuthenticationStateProvider _auth;

    public ReviewService(AppDbContext db, AuthenticationStateProvider auth)
    {
        _db = db;
        _auth = auth;
    }

    private async Task<int> GetUserId()
    {
        var authState = await _auth.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated == true)
            throw new Exception("User not authenticated");

        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (id == null)
            throw new Exception("User ID not found");

        return int.Parse(id);
    }

    public async Task SubmitReview(
        int serviceRequestId,
        int rating,
        List<int> tagIds,
        string? comment)
    {
        var userId = await GetUserId();

        var service = await _db.ServiceRequests
            .FirstOrDefaultAsync(s => s.Id == serviceRequestId);

        if (service == null)
            throw new Exception("Service not found");

        if (service.CustomerId != userId)
            throw new Exception("Unauthorized");

        if (service.Status != ServiceRequestStatus.Completed)
            throw new Exception("Service not completed");

        var alreadyReviewed = await _db.Reviews
            .AnyAsync(r => r.ServiceRequestId == serviceRequestId);

        if (alreadyReviewed)
            throw new Exception("Already reviewed");

        var review = new Review
        {
            ServiceRequestId = serviceRequestId,
            CustomerId = userId,
            ProfessionalId = (int)service.ProfessionalId!,
            Rating = rating,
            Comment = comment,
            Tags = tagIds.Select(id => (ReviewTag)id).ToList()
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
    }
}