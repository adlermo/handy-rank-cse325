using HandyRank.Data;
using HandyRank.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HandyRank.Features.Gamification.Services;

public class GamificationService
{
    private readonly AppDbContext _db;

    public GamificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task ApplyReviewXP(int reviewId)
    {
        var review = await _db.Reviews
            .Include(r => r.ServiceRequest)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null) return;

        if (review.XPGranted) return;

        var profile = await _db.HandymanProfiles
            .FirstOrDefaultAsync(p => p.UserId == review.ProfessionalId);

        if (profile == null) return;

        if (review?.ServiceRequest == null) return;
        var baseXP = GetBaseXP(review.ServiceRequest.Size);

        // 2. Multiplicador
        var multiplier = GetMultiplier(review.Rating);

        // 3. XP final
        var gainedXP = (int)Math.Round(baseXP * multiplier);

        // 4. Aplicar
        profile.XP += gainedXP;
        profile.TotalJobsCompleted += 1;

        // 5. Atualizar média
        var ratings = await _db.Reviews
            .Where(r => r.ProfessionalId == review.ProfessionalId)
            .Select(r => r.Rating)
            .ToListAsync();

        profile.RatingAverage = ratings.Count > 0
            ? ratings.Average()
            : 0;

        // 6. Rank
        profile.Rank = CalculateRank(profile.XP);

        // 7. Marcar como aplicado
        review.XPGranted = true;

        await _db.SaveChangesAsync();
    }

    private int GetBaseXP(JobSize size)
    {
        return size switch
        {
            JobSize.Small => 40,
            JobSize.Medium => 70,
            JobSize.Large => 100,
            _ => 50
        };
    }

    private double GetMultiplier(int rating)
    {
        return rating switch
        {
            5 => 1.5,
            4 => 1.2,
            3 => 1.0,
            _ => 0.5
        };
    }

    private string CalculateRank(int xp)
    {
        return xp switch
        {
            < 200 => "Rookie",
            < 500 => "Novice",
            < 1000 => "Apprentice",
            < 2000 => "Skilled",
            < 4000 => "Expert",
            < 7000 => "Master",
            _ => "Grand Master"
        };
    }
}