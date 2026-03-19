using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<FoodLog> FoodLogs => Set<FoodLog>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<MealEntry> MealEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.OwnerUserId).IsRequired();
            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(f => new { f.OwnerUserId, f.Name });
        });

        modelBuilder.Entity<FoodLog>(entity =>
        {
            entity.HasKey(fl => fl.Id);
            entity.HasOne(fl => fl.User)
                .WithMany(u => u.FoodLogs)
                .HasForeignKey(fl => fl.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(fl => fl.Food)
                .WithMany(f => f.FoodLogs)
                .HasForeignKey(fl => fl.FoodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(up => up.Id);
            entity.Property(up => up.FullName).IsRequired().HasMaxLength(200);
            entity.HasIndex(up => up.UserId).IsUnique();
        });
    }
}
