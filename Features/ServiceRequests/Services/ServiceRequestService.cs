using HandyRank.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HandyRank.Features.ServiceRequests.Services;

public class ServiceRequestService
{
    private readonly AppDbContext _db;
    private readonly AuthenticationStateProvider _auth;

    public ServiceRequestService(AppDbContext db, AuthenticationStateProvider auth)
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
            throw new Exception("User ID claim not found");

        return int.Parse(id);
    }
    public async Task<List<ServiceRequest>> GetCustomerRequests()
    {
        var userId = await GetUserId();

        return await _db.ServiceRequests
            .Include(r => r.Category)
            .Where(r => r.CustomerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateCustomerRequest(CreateServiceRequestDto dto)
    {
        Console.WriteLine($"CategoryId: {dto.CategoryId}");
        var userId = await GetUserId();

        var exists = await _db.ServiceCategories
            .AnyAsync(c => c.Id == dto.CategoryId);

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

        _db.ServiceRequests.Add(request);
        await _db.SaveChangesAsync();
    }
}