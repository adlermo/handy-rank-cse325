using HandyRank.Data;
using HandyRank.Models;
using Microsoft.EntityFrameworkCore;

namespace HandyRank.Domain.Categories;

public class CategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ServiceCategory>> GetAll()
    {
        return await _db.ServiceCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ServiceCategory?> GetById(int id)
    {
        return await _db.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> Exists(int id)
    {
        return await _db.ServiceCategories
            .AnyAsync(c => c.Id == id);
    }
}