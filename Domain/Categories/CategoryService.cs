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

    // 🔍 GET ALL (dropdown, filters, etc)
    public async Task<List<ServiceCategory>> GetAll()
    {
        return await _db.ServiceCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // 🔎 GET BY ID (futuro: validações, matching, etc)
    public async Task<ServiceCategory?> GetById(int id)
    {
        return await _db.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // 🧠 VALIDATION (útil pro CreateRequest)
    public async Task<bool> Exists(int id)
    {
        return await _db.ServiceCategories
            .AnyAsync(c => c.Id == id);
    }
}