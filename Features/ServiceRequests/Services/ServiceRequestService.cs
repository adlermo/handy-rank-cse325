using HandyRank.Data;
using Microsoft.EntityFrameworkCore;

namespace HandyRank.Features.ServiceRequests.Services;

public class ServiceRequestService
{
    private readonly AppDbContext _db;

    public ServiceRequestService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> CategoryExists(int categoryId)
    {
        return await _db.ServiceCategories
            .AnyAsync(c => c.Id == categoryId);
    }

    public async Task Add(ServiceRequest request)
    {
        _db.ServiceRequests.Add(request);
        await _db.SaveChangesAsync();
    }
}