using LIOSCare.DoctorDashboard.Domain.Entities;
using LIOSCare.DoctorDashboard.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;

namespace LIOSCare.DoctorDashboard.Infrastructure.Persistence;

public sealed class DoctorPortalDbContext(DbContextOptions<DoctorPortalDbContext> options) : DbContext(options)
{
    public DbSet<DoctorAccount> DoctorAccounts => Set<DoctorAccount>();
    public DbSet<DoctorRefreshToken> DoctorRefreshTokens => Set<DoctorRefreshToken>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<DoctorEducation> DoctorEducations => Set<DoctorEducation>();
    public DbSet<ServiceTier> ServiceTiers => Set<ServiceTier>();
    public DbSet<QuickChatRequest> QuickChatRequests => Set<QuickChatRequest>();
    public DbSet<DirectBookingRequest> DirectBookingRequests => Set<DirectBookingRequest>();
    public DbSet<SessionReport> SessionReports => Set<SessionReport>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<DoctorAccount>(entity =>
        {
            entity.ToTable("doctor_dashboard_accounts", "auth");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(600).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.DoctorProfile).WithOne(x => x.Account).HasForeignKey<DoctorProfile>(x => x.DoctorAccountId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DoctorRefreshToken>(entity =>
        {
            entity.ToTable("doctor_refresh_tokens", "auth");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.RevokedReason).HasMaxLength(160);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.DoctorAccountId, x.ExpiresAt });
            entity.HasOne(x => x.DoctorAccount).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.DoctorAccountId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DoctorProfile>(entity =>
        {
            entity.ToTable("doctor_profiles", "provider");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Specializations).HasColumnType("text[]");
            entity.Property(x => x.Certifications).HasColumnType("text[]");
            entity.Property(x => x.Bio).HasMaxLength(1000);
            entity.Property(x => x.ProfilePhotoUrl).HasMaxLength(500);
            entity.Property(x => x.Rating).HasPrecision(3, 2);
            entity.HasIndex(x => x.IsAvailable);
            entity.HasIndex(x => x.DoctorAccountId).IsUnique();
        });

        modelBuilder.Entity<DoctorEducation>(entity =>
        {
            entity.ToTable("doctor_educations", "provider");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Degree).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Institution).HasMaxLength(160).IsRequired();
            entity.HasIndex(x => x.DoctorId);
            entity.HasOne(x => x.Doctor).WithMany(x => x.Education).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceTier>(entity =>
        {
            entity.ToTable("service_tiers", "provider");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(40).IsRequired();
            entity.Property(x => x.PriceUsd).HasPrecision(10, 2);
            entity.Property(x => x.Features).HasColumnType("text[]");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => new { x.IsActive, x.PriceUsd });
        });

        modelBuilder.Entity<QuickChatRequest>(entity =>
        {
            entity.ToTable("quick_chat_requests", "provider");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserFirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.UserLastInitial).HasMaxLength(5).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.UserMessage).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.DoctorReply).HasMaxLength(4000);
            entity.HasIndex(x => new { x.Status, x.SlaDeadline });
            entity.HasIndex(x => new { x.DoctorId, x.Status, x.CreatedAt });
            entity.HasIndex(x => new { x.TierId, x.Status });
            entity.HasOne(x => x.Doctor).WithMany(x => x.QuickChats).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Tier).WithMany().HasForeignKey(x => x.TierId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DirectBookingRequest>(entity =>
        {
            entity.ToTable("direct_booking_requests", "provider");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserFullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.UserPhotoUrl).HasMaxLength(500);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.DeclineReason).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.AmountUsd).HasPrecision(10, 2);
            entity.Property(x => x.PlatformFeeUsd).HasPrecision(10, 2);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.DeclineNote).HasMaxLength(500);
            entity.HasIndex(x => new { x.DoctorId, x.Status, x.CreatedAt });
            entity.HasIndex(x => new { x.UserId, x.DoctorId });
            entity.HasOne(x => x.Doctor).WithMany(x => x.Bookings).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Tier).WithMany().HasForeignKey(x => x.TierId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SessionReport>(entity =>
        {
            entity.ToTable("doctor_session_reports", "provider");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserFullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.SessionDate).HasColumnType("date");
            entity.Property(x => x.ProgressNotes).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.NextSteps).HasMaxLength(500);
            entity.Property(x => x.TechniquesUsed).HasColumnType("text[]");
            entity.HasIndex(x => new { x.DoctorId, x.SessionDate });
            entity.HasIndex(x => new { x.BookingId, x.SessionNumber }).IsUnique();
            entity.HasOne(x => x.Booking).WithMany(x => x.Reports).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens", "auth");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.DoctorAccountId);
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasOne(x => x.Account).WithMany(x => x.PasswordResetTokens)
                .HasForeignKey(x => x.DoctorAccountId).OnDelete(DeleteBehavior.Cascade);
        });

        ApplySnakeCase(modelBuilder);
    }

    private static void ApplySnakeCase(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName() ?? string.Empty));
            }
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? string.Empty));
            }
            foreach (var fk in entity.GetForeignKeys())
            {
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName() ?? string.Empty));
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        var snake = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2").Replace("__", "_").ToLowerInvariant();
        return snake;
    }
}
