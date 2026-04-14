using Microsoft.EntityFrameworkCore;
using HandyRank.Models;

namespace HandyRank.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<HandymanProfile> HandymanProfiles => Set<HandymanProfile>();
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<User> Users => Set<User>();
    public DbSet<Review> Reviews { get; set; }

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

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasIndex(r => r.CustomerId);
            entity.HasIndex(r => r.Status);
            entity.HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceCategory>().HasData(
            new ServiceCategory { Id = 1, Name = "Electrical" },
            new ServiceCategory { Id = 2, Name = "Plumbing" },
            new ServiceCategory { Id = 3, Name = "Painting" }
        );

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

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasIndex(a => new { a.ServiceRequestId, a.ProfessionalId })
                .IsUnique();

            entity.HasOne(a => a.ServiceRequest)
                .WithMany()
                .HasForeignKey(a => a.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Professional)
                .WithMany()
                .HasForeignKey(a => a.ProfessionalId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Review>()
            .Property(r => r.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => Enum.Parse<ReviewTag>(x))
                      .ToList()
            );
    }
}
