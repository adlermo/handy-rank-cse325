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

    public async Task<List<JobCardDto>> GetAvailableJobs()
    {
        var userId = await GetUserId();

        var appliedIds = await _db.JobApplications
    .Where(a => a.ProfessionalId == userId)
    .Select(a => a.ServiceRequestId)
    .ToListAsync();

        return await _db.ServiceRequests
            .Include(r => r.Category)
            .Where(r => r.Status == ServiceRequestStatus.Open)
            .Select(r => new JobCardDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Category = r.Category!.Name,
                Location = r.Location,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
                HasApplied = appliedIds.Contains(r.Id)
            })
            .ToListAsync();
    }

    public async Task<List<JobCardDto>> GetMyJobs()
    {
        var userId = await GetUserId();

        return await _db.ServiceRequests
            .Include(r => r.Category)
            .Where(r => r.ProfessionalId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new JobCardDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Category = r.Category!.Name,
                Location = r.Location,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
            })
            .ToListAsync();
    }

    public async Task ApplyToJob(int requestId)
    {
        var userId = await GetUserId();

        var job = await _db.ServiceRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (job == null)
            throw new Exception("Job not found");

        if (job.Status != ServiceRequestStatus.Open)
            throw new Exception("Job not open");

        var alreadyApplied = await _db.JobApplications
            .AnyAsync(a => a.ServiceRequestId == requestId && a.ProfessionalId == userId);

        if (alreadyApplied)
            throw new Exception("Already applied");

        var application = new JobApplication
        {
            ServiceRequestId = requestId,
            ProfessionalId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = JobApplicationStatus.Pending
        };

        _db.JobApplications.Add(application);
        await _db.SaveChangesAsync();
    }
}