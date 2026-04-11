using Microsoft.EntityFrameworkCore;
using HandyRank.Models;

namespace HandyRank.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Job> Jobs => Set<Job>();

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

            entity.Property(user => user.Role)
                .HasConversion<string>();
        });

        modelBuilder.Entity<Job>()
            .Property(job => job.Status)
            .HasConversion<string>();
    }
}
