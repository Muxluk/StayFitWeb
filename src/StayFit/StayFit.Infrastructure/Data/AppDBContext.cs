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
    public DbSet<NutritionGoal> NutritionGoals => Set<NutritionGoal>();
    public DbSet<FoodCategoryEntity> FoodCategories => Set<FoodCategoryEntity>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SecurityLogEntry> SecurityLogs => Set<SecurityLogEntry>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportTicketReply> SupportTicketReplies => Set<SupportTicketReply>();

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
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NutritionGoal>(entity =>
        {
            entity.HasKey(ng => ng.Id);
            entity.HasIndex(ng => ng.UserId).IsUnique();
            entity.Property(ng => ng.UserId).IsRequired();
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(up => up.Id);
            entity.Property(up => up.FullName).IsRequired().HasMaxLength(200);
            entity.HasIndex(up => up.UserId).IsUnique();
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(us => us.Id);
            entity.Property(us => us.SessionToken).IsRequired().HasMaxLength(64);
            entity.HasIndex(us => us.SessionToken).IsUnique();
            entity.HasIndex(us => us.UserId);
            entity.Property(us => us.IpAddress).HasMaxLength(64);
            entity.Property(us => us.UserAgent).HasMaxLength(512);
            // Немає FK на AspNetUsers — зв'язок через DomainUsers
            entity.HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(500);
            entity.Property(n => n.Type).IsRequired().HasMaxLength(50);
            entity.HasIndex(n => n.UserId);
            entity.HasIndex(n => new { n.UserId, n.IsRead });
        });

        modelBuilder.Entity<SecurityLogEntry>(entity =>
        {
            entity.HasKey(sl => sl.Id);
            entity.Property(sl => sl.UserId).IsRequired();
            entity.Property(sl => sl.EventType).IsRequired().HasMaxLength(50);
            entity.Property(sl => sl.Description).IsRequired().HasMaxLength(200);
            entity.Property(sl => sl.IpAddress).HasMaxLength(64);
            entity.Property(sl => sl.UserAgent).HasMaxLength(512);
            entity.Property(sl => sl.Status).IsRequired().HasMaxLength(20);
            entity.Property(sl => sl.AdditionalInfo).HasMaxLength(500);
            entity.HasIndex(sl => sl.UserId);
            entity.HasIndex(sl => new { sl.UserId, sl.CreatedAt }).IsDescending(false, true);
            entity.HasOne(sl => sl.User)
                .WithMany()
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.UserId).IsRequired();
            entity.Property(t => t.Subject).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Message).IsRequired().HasMaxLength(2000);
            entity.Property(t => t.Status).IsRequired().HasMaxLength(20);
            entity.Property(t => t.CreatedAt).IsRequired();
            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => new { t.UserId, t.Status });
            entity.HasIndex(t => t.CreatedAt);
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(t => t.Replies)
                .WithOne(r => r.Ticket)
                .HasForeignKey(r => r.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupportTicketReply>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TicketId).IsRequired();
            entity.Property(r => r.Message).IsRequired().HasMaxLength(2000);
            entity.Property(r => r.CreatedAt).IsRequired();
            entity.Property(r => r.IsAdminReply).IsRequired();
            entity.HasIndex(r => r.TicketId);
            entity.HasIndex(r => r.CreatedAt);
        });
    }
}
