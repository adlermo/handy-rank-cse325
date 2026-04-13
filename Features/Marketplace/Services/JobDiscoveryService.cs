using HandyRank.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace HandyRank.Features.Marketplace.Services;

public class JobDiscoveryService
{
    private readonly AppDbContext _db;

    private readonly AuthenticationStateProvider _auth;

    public JobDiscoveryService(AppDbContext db, AuthenticationStateProvider auth)
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

    public async Task<List<ServiceRequest>> GetAvailableJobs()
    {
        return await _db.ServiceRequests
            .Include(r => r.Category)
            .Where(r => r.Status == ServiceRequestStatus.Open)
            // .Where(r => professionalCategories.Contains(r.CategoryId))
            // .Where(r => r.Location == userLocation)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ServiceRequest>> GetMyJobs()
    {
        var userId = await GetUserId();

        return await _db.ServiceRequests
            .Include(r => r.Category)
            .Where(r => r.ProfessionalId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task AcceptJob(int requestId)
    {
        var userId = await GetUserId();

        var job = await _db.ServiceRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (job == null)
            throw new Exception("Job not found");

        if (job.Status != ServiceRequestStatus.Open)
            throw new Exception("Job already taken");

        job.Status = ServiceRequestStatus.Pending;
        job.ProfessionalId = userId;

        await _db.SaveChangesAsync();
    }
}