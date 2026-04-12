using HandyRank.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HandyRank.Features.ServiceRequests.Services;

public class CustomerRequestService
{
    private readonly AppDbContext _db;
    private readonly AuthenticationStateProvider _auth;
    private readonly ServiceRequestService _base;

    public CustomerRequestService(
        AppDbContext db,
        AuthenticationStateProvider auth,
        ServiceRequestService baseService)
    {
        _db = db;
        _auth = auth;
        _base = baseService;
    }

    private async Task<int> GetUserId()
    {
        var authState = await _auth.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated == true)
            throw new Exception("User not authenticated");

        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (id == null)
            throw new Exception("User ID claim not found");

        return int.Parse(id);
    }

    public async Task<List<ServiceRequest>> GetMyRequests()
    {
        var userId = await GetUserId();

        return await _db.ServiceRequests
            .Include(r => r.Category)
            .Where(r => r.CustomerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task Create(CreateServiceRequestDto dto)
    {
        var userId = await GetUserId();

        var exists = await _base.CategoryExists(dto.CategoryId);

        if (!exists)
            throw new Exception("Invalid category");

        var request = new ServiceRequest
        {
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            CategoryId = dto.CategoryId,
            CustomerId = userId
        };

        await _base.Add(request);
    }

    public async Task Delete(int id)
    {
        var userId = await GetUserId();

        var request = await _db.ServiceRequests
            .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == userId);

        if (request == null)
            throw new Exception("Request not found");

        _db.ServiceRequests.Remove(request);
        await _db.SaveChangesAsync();
    }
}