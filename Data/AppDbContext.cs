using Microsoft.EntityFrameworkCore;
using HandyRank.Models;

namespace HandyRank.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<HandymanProfile> HandymanProfiles => Set<HandymanProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.Email)
                .HasMaxLength(256);

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(512);

            entity.Property(user => user.Name)
                .HasMaxLength(User.MaxNameLength);

            entity.Property(user => user.Bio)
                .HasMaxLength(User.MaxBioLength);

            entity.Property(user => user.Location)
                .HasMaxLength(User.MaxLocationLength);

            entity.Property(user => user.Role)
                .HasConversion<string>();
        });

        modelBuilder.Entity<Job>()
            .Property(job => job.Status)
            .HasConversion<string>();

        modelBuilder.Entity<HandymanProfile>(entity =>
        {
            entity.HasIndex(profile => profile.UserId)
                .IsUnique();

            entity.Property(profile => profile.Skills)
                .HasMaxLength(HandymanProfile.MaxSkillsLength);

            entity.Property(profile => profile.Rank)
                .HasMaxLength(HandymanProfile.MaxRankLength);

            entity.HasOne(profile => profile.User)
                .WithOne(user => user.HandymanProfile)
                .HasForeignKey<HandymanProfile>(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
