using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using LIOSCare.DoctorDashboard.Domain.Enums;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;

#nullable disable

namespace LIOSCare.DoctorDashboard.Infrastructure.Migrations;

[DbContext(typeof(DoctorPortalDbContext))]
partial class DoctorPortalDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DoctorAccount", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<string>("Email").IsRequired().HasMaxLength(180).HasColumnType("character varying(180)").HasColumnName("email");
            b.Property<bool>("EmailConfirmed").HasColumnType("boolean").HasColumnName("email_confirmed");
            b.Property<bool>("IsActive").HasColumnType("boolean").HasColumnName("is_active");
            b.Property<DateTimeOffset?>("LastLoginAt").HasColumnType("timestamp with time zone").HasColumnName("last_login_at");
            b.Property<string>("PasswordHash").IsRequired().HasMaxLength(600).HasColumnType("character varying(600)").HasColumnName("password_hash");
            b.HasKey("Id").HasName("pk_doctor_dashboard_accounts");
            b.HasIndex("Email").IsUnique().HasDatabaseName("ix_doctor_dashboard_accounts_email");
            b.ToTable("doctor_dashboard_accounts", "auth");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.PasswordResetToken", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<Guid>("DoctorAccountId").HasColumnType("uuid").HasColumnName("doctor_account_id");
            b.Property<string>("TokenHash").IsRequired().HasMaxLength(64).HasColumnType("character varying(64)").HasColumnName("token_hash");
            b.Property<DateTimeOffset>("ExpiresAt").HasColumnType("timestamp with time zone").HasColumnName("expires_at");
            b.Property<DateTimeOffset?>("UsedAt").HasColumnType("timestamp with time zone").HasColumnName("used_at");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.HasKey("Id").HasName("pk_password_reset_tokens");
            b.HasIndex("TokenHash").IsUnique().HasDatabaseName("ix_password_reset_tokens_token_hash");
            b.HasIndex("DoctorAccountId").HasDatabaseName("ix_password_reset_tokens_doctor_account_id");
            b.HasIndex("ExpiresAt").HasDatabaseName("ix_password_reset_tokens_expires_at");
            b.ToTable("password_reset_tokens", "auth");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DoctorRefreshToken", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<Guid>("DoctorAccountId").HasColumnType("uuid").HasColumnName("doctor_account_id");
            b.Property<DateTimeOffset>("ExpiresAt").HasColumnType("timestamp with time zone").HasColumnName("expires_at");
            b.Property<DateTimeOffset?>("RevokedAt").HasColumnType("timestamp with time zone").HasColumnName("revoked_at");
            b.Property<string>("RevokedReason").HasMaxLength(160).HasColumnType("character varying(160)").HasColumnName("revoked_reason");
            b.Property<string>("TokenHash").IsRequired().HasMaxLength(256).HasColumnType("character varying(256)").HasColumnName("token_hash");
            b.HasKey("Id").HasName("pk_doctor_refresh_tokens");
            b.HasIndex(new[] { "DoctorAccountId", "ExpiresAt" }).HasDatabaseName("ix_doctor_refresh_tokens_doctor_account_id_expires_at");
            b.HasIndex("TokenHash").IsUnique().HasDatabaseName("ix_doctor_refresh_tokens_token_hash");
            b.ToTable("doctor_refresh_tokens", "auth");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.ServiceTier", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<string[]>("Features").IsRequired().HasColumnType("text[]").HasColumnName("features");
            b.Property<bool>("IsActive").HasColumnType("boolean").HasColumnName("is_active");
            b.Property<string>("Name").IsRequired().HasMaxLength(40).HasColumnType("character varying(40)").HasColumnName("name");
            b.Property<decimal>("PriceUsd").HasPrecision(10, 2).HasColumnType("numeric(10,2)").HasColumnName("price_usd");
            b.Property<int>("ResponseWindowHours").HasColumnType("integer").HasColumnName("response_window_hours");
            b.HasKey("Id").HasName("pk_service_tiers");
            b.HasIndex(new[] { "IsActive", "PriceUsd" }).HasDatabaseName("ix_service_tiers_is_active_price_usd");
            b.HasIndex("Name").IsUnique().HasDatabaseName("ix_service_tiers_name");
            b.ToTable("service_tiers", "provider");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DoctorProfile", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<string>("Bio").IsRequired().HasMaxLength(1000).HasColumnType("character varying(1000)").HasColumnName("bio");
            b.Property<string[]>("Certifications").IsRequired().HasColumnType("text[]").HasColumnName("certifications");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<Guid>("DoctorAccountId").HasColumnType("uuid").HasColumnName("doctor_account_id");
            b.Property<string>("FullName").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)").HasColumnName("full_name");
            b.Property<bool>("IsAvailable").HasColumnType("boolean").HasColumnName("is_available");
            b.Property<int>("PatientCount").HasColumnType("integer").HasColumnName("patient_count");
            b.Property<string>("ProfilePhotoUrl").HasMaxLength(500).HasColumnType("character varying(500)").HasColumnName("profile_photo_url");
            b.Property<decimal>("Rating").HasPrecision(3, 2).HasColumnType("numeric(3,2)").HasColumnName("rating");
            b.Property<string[]>("Specializations").IsRequired().HasColumnType("text[]").HasColumnName("specializations");
            b.Property<DateTimeOffset>("UpdatedAt").HasColumnType("timestamp with time zone").HasColumnName("updated_at");
            b.Property<int>("YearsExperience").HasColumnType("integer").HasColumnName("years_experience");
            b.HasKey("Id").HasName("pk_doctor_profiles");
            b.HasIndex("DoctorAccountId").IsUnique().HasDatabaseName("ix_doctor_profiles_doctor_account_id");
            b.HasIndex("IsAvailable").HasDatabaseName("ix_doctor_profiles_is_available");
            b.ToTable("doctor_profiles", "provider");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DoctorEducation", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<string>("Degree").IsRequired().HasMaxLength(160).HasColumnType("character varying(160)").HasColumnName("degree");
            b.Property<Guid>("DoctorId").HasColumnType("uuid").HasColumnName("doctor_id");
            b.Property<string>("Institution").IsRequired().HasMaxLength(160).HasColumnType("character varying(160)").HasColumnName("institution");
            b.Property<int>("Year").HasColumnType("integer").HasColumnName("year");
            b.HasKey("Id").HasName("pk_doctor_educations");
            b.HasIndex("DoctorId").HasDatabaseName("ix_doctor_educations_doctor_id");
            b.ToTable("doctor_educations", "provider");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DirectBookingRequest", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<DateTimeOffset?>("AcceptedAt").HasColumnType("timestamp with time zone").HasColumnName("accepted_at");
            b.Property<decimal>("AmountUsd").HasPrecision(10, 2).HasColumnType("numeric(10,2)").HasColumnName("amount_usd");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<DateTimeOffset?>("DeclinedAt").HasColumnType("timestamp with time zone").HasColumnName("declined_at");
            b.Property<string>("DeclineNote").HasMaxLength(500).HasColumnType("character varying(500)").HasColumnName("decline_note");
            b.Property<DeclineReason>("DeclineReason").HasConversion<string>().IsRequired().HasMaxLength(64).HasColumnType("character varying(64)").HasColumnName("decline_reason");
            b.Property<Guid>("DoctorId").HasColumnType("uuid").HasColumnName("doctor_id");
            b.Property<string>("Notes").HasMaxLength(2000).HasColumnType("character varying(2000)").HasColumnName("notes");
            b.Property<PaymentStatus>("PaymentStatus").HasConversion<string>().IsRequired().HasMaxLength(24).HasColumnType("character varying(24)").HasColumnName("payment_status");
            b.Property<decimal>("PlatformFeeUsd").HasPrecision(10, 2).HasColumnType("numeric(10,2)").HasColumnName("platform_fee_usd");
            b.Property<DateTimeOffset?>("ScheduledAt").HasColumnType("timestamp with time zone").HasColumnName("scheduled_at");
            b.Property<BookingStatus>("Status").HasConversion<string>().IsRequired().HasMaxLength(24).HasColumnType("character varying(24)").HasColumnName("status");
            b.Property<Guid>("TierId").HasColumnType("uuid").HasColumnName("tier_id");
            b.Property<Guid>("UserId").HasColumnType("uuid").HasColumnName("user_id");
            b.Property<string>("UserFullName").IsRequired().HasMaxLength(120).HasColumnType("character varying(120)").HasColumnName("user_full_name");
            b.Property<string>("UserPhotoUrl").HasMaxLength(500).HasColumnType("character varying(500)").HasColumnName("user_photo_url");
            b.HasKey("Id").HasName("pk_direct_booking_requests");
            b.HasIndex(new[] { "DoctorId", "Status", "CreatedAt" }).HasDatabaseName("ix_direct_booking_requests_doctor_id_status_created_at");
            b.HasIndex("TierId").HasDatabaseName("ix_direct_booking_requests_tier_id");
            b.HasIndex(new[] { "UserId", "DoctorId" }).HasDatabaseName("ix_direct_booking_requests_user_id_doctor_id");
            b.ToTable("direct_booking_requests", "provider");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.QuickChatRequest", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<DateTimeOffset?>("AcceptedAt").HasColumnType("timestamp with time zone").HasColumnName("accepted_at");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<Guid?>("DoctorId").HasColumnType("uuid").HasColumnName("doctor_id");
            b.Property<string>("DoctorReply").HasMaxLength(4000).HasColumnType("character varying(4000)").HasColumnName("doctor_reply");
            b.Property<Guid?>("MessagingThreadId").HasColumnType("uuid").HasColumnName("messaging_thread_id");
            b.Property<DateTimeOffset?>("RepliedAt").HasColumnType("timestamp with time zone").HasColumnName("replied_at");
            b.Property<DateTimeOffset>("SlaDeadline").HasColumnType("timestamp with time zone").HasColumnName("sla_deadline");
            b.Property<QuickChatStatus>("Status").HasConversion<string>().IsRequired().HasMaxLength(24).HasColumnType("character varying(24)").HasColumnName("status");
            b.Property<Guid>("TierId").HasColumnType("uuid").HasColumnName("tier_id");
            b.Property<string>("UserFirstName").IsRequired().HasMaxLength(80).HasColumnType("character varying(80)").HasColumnName("user_first_name");
            b.Property<string>("UserLastInitial").IsRequired().HasMaxLength(5).HasColumnType("character varying(5)").HasColumnName("user_last_initial");
            b.Property<string>("UserMessage").IsRequired().HasMaxLength(4000).HasColumnType("character varying(4000)").HasColumnName("user_message");
            b.Property<Guid>("UserId").HasColumnType("uuid").HasColumnName("user_id");
            b.HasKey("Id").HasName("pk_quick_chat_requests");
            b.HasIndex(new[] { "DoctorId", "Status", "CreatedAt" }).HasDatabaseName("ix_quick_chat_requests_doctor_id_status_created_at");
            b.HasIndex(new[] { "Status", "SlaDeadline" }).HasDatabaseName("ix_quick_chat_requests_status_sla_deadline");
            b.HasIndex(new[] { "TierId", "Status" }).HasDatabaseName("ix_quick_chat_requests_tier_id_status");
            b.ToTable("quick_chat_requests", "provider");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.SessionReport", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<Guid>("BookingId").HasColumnType("uuid").HasColumnName("booking_id");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<Guid>("DoctorId").HasColumnType("uuid").HasColumnName("doctor_id");
            b.Property<int>("GoalsCompleted").HasColumnType("integer").HasColumnName("goals_completed");
            b.Property<int>("GoalsTotal").HasColumnType("integer").HasColumnName("goals_total");
            b.Property<int>("MoodAfter").HasColumnType("integer").HasColumnName("mood_after");
            b.Property<int>("MoodBefore").HasColumnType("integer").HasColumnName("mood_before");
            b.Property<string>("NextSteps").HasMaxLength(500).HasColumnType("character varying(500)").HasColumnName("next_steps");
            b.Property<string>("ProgressNotes").IsRequired().HasMaxLength(2000).HasColumnType("character varying(2000)").HasColumnName("progress_notes");
            b.Property<DateTime>("SessionDate").HasColumnType("date").HasColumnName("session_date");
            b.Property<int>("SessionNumber").HasColumnType("integer").HasColumnName("session_number");
            b.Property<string[]>("TechniquesUsed").IsRequired().HasColumnType("text[]").HasColumnName("techniques_used");
            b.Property<DateTimeOffset>("UpdatedAt").HasColumnType("timestamp with time zone").HasColumnName("updated_at");
            b.Property<Guid>("UserId").HasColumnType("uuid").HasColumnName("user_id");
            b.Property<string>("UserFullName").IsRequired().HasMaxLength(120).HasColumnType("character varying(120)").HasColumnName("user_full_name");
            b.HasKey("Id").HasName("pk_session_reports");
            b.HasIndex(new[] { "BookingId", "SessionNumber" }).IsUnique().HasDatabaseName("ix_session_reports_booking_id_session_number");
            b.HasIndex(new[] { "DoctorId", "SessionDate" }).HasDatabaseName("ix_session_reports_doctor_id_session_date");
            b.ToTable("doctor_session_reports", "provider");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DoctorRefreshToken", b =>
        {
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.DoctorAccount", "DoctorAccount")
                .WithMany("RefreshTokens")
                .HasForeignKey("DoctorAccountId")
                .HasConstraintName("fk_doctor_refresh_tokens_doctor_dashboard_accounts_doctor_account_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("DoctorAccount");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DoctorProfile", b =>
        {
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.DoctorAccount", "Account")
                .WithOne("DoctorProfile")
                .HasForeignKey("LIOSCare.DoctorDashboard.Domain.Entities.DoctorProfile", "DoctorAccountId")
                .HasConstraintName("fk_doctor_profiles_doctor_dashboard_accounts_doctor_account_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Account");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DoctorEducation", b =>
        {
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.DoctorProfile", "Doctor")
                .WithMany("Education")
                .HasForeignKey("DoctorId")
                .HasConstraintName("fk_doctor_educations_doctor_profiles_doctor_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Doctor");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.QuickChatRequest", b =>
        {
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.DoctorProfile", "Doctor")
                .WithMany("QuickChats")
                .HasForeignKey("DoctorId")
                .HasConstraintName("fk_quick_chat_requests_doctor_profiles_doctor_id")
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.ServiceTier", "Tier")
                .WithMany()
                .HasForeignKey("TierId")
                .HasConstraintName("fk_quick_chat_requests_service_tiers_tier_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.Navigation("Doctor");
            b.Navigation("Tier");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DirectBookingRequest", b =>
        {
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.DoctorProfile", "Doctor")
                .WithMany("Bookings")
                .HasForeignKey("DoctorId")
                .HasConstraintName("fk_direct_booking_requests_doctor_profiles_doctor_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.ServiceTier", "Tier")
                .WithMany()
                .HasForeignKey("TierId")
                .HasConstraintName("fk_direct_booking_requests_service_tiers_tier_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.Navigation("Doctor");
            b.Navigation("Tier");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.SessionReport", b =>
        {
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.DirectBookingRequest", "Booking")
                .WithMany("Reports")
                .HasForeignKey("BookingId")
                .HasConstraintName("fk_session_reports_direct_booking_requests_booking_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.DoctorProfile", "Doctor")
                .WithMany()
                .HasForeignKey("DoctorId")
                .HasConstraintName("fk_session_reports_doctor_profiles_doctor_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.Navigation("Booking");
            b.Navigation("Doctor");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.PasswordResetToken", b =>
        {
            b.HasOne("LIOSCare.DoctorDashboard.Domain.Entities.DoctorAccount", "Account")
                .WithMany("PasswordResetTokens")
                .HasForeignKey("DoctorAccountId")
                .HasConstraintName("fk_password_reset_tokens_doctor_dashboard_accounts_doctor_account_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Account");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DoctorAccount", b =>
        {
            b.Navigation("DoctorProfile");
            b.Navigation("PasswordResetTokens");
            b.Navigation("RefreshTokens");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DoctorProfile", b =>
        {
            b.Navigation("Bookings");
            b.Navigation("Education");
            b.Navigation("QuickChats");
        });

        modelBuilder.Entity("LIOSCare.DoctorDashboard.Domain.Entities.DirectBookingRequest", b =>
        {
            b.Navigation("Reports");
        });
    }
}
