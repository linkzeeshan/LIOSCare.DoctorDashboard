using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using LIOSCare.DoctorDashboard.Domain.Enums;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;

#nullable disable

namespace LIOSCare.DoctorDashboard.Infrastructure.Migrations;

[DbContext(typeof(DoctorPortalDbContext))]
[Migration("202605080001_InitialDoctorDashboard")]
public partial class InitialDoctorDashboard : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "auth");
        migrationBuilder.EnsureSchema(name: "provider");
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

        migrationBuilder.CreateTable(
            name: "doctor_dashboard_accounts",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                password_hash = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table => table.PrimaryKey("pk_doctor_dashboard_accounts", x => x.id));

        migrationBuilder.CreateTable(
            name: "service_tiers",
            schema: "provider",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                price_usd = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                features = table.Column<string[]>(type: "text[]", nullable: false),
                response_window_hours = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_service_tiers", x => x.id));

        migrationBuilder.CreateTable(
            name: "doctor_profiles",
            schema: "provider",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                doctor_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                specializations = table.Column<string[]>(type: "text[]", nullable: false),
                years_experience = table.Column<int>(type: "integer", nullable: false),
                rating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                patient_count = table.Column<int>(type: "integer", nullable: false),
                bio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                certifications = table.Column<string[]>(type: "text[]", nullable: false),
                profile_photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                is_available = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_doctor_profiles", x => x.id);
                table.ForeignKey("fk_doctor_profiles_doctor_dashboard_accounts_doctor_account_id", x => x.doctor_account_id, "doctor_dashboard_accounts", "id", "auth", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "doctor_refresh_tokens",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                doctor_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                revoked_reason = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_doctor_refresh_tokens", x => x.id);
                table.ForeignKey("fk_doctor_refresh_tokens_doctor_dashboard_accounts_doctor_account_id", x => x.doctor_account_id, "doctor_dashboard_accounts", "id", "auth", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "doctor_educations",
            schema: "provider",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                degree = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                institution = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                year = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_doctor_educations", x => x.id);
                table.ForeignKey("fk_doctor_educations_doctor_profiles_doctor_id", x => x.doctor_id, "doctor_profiles", "id", "provider", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "direct_booking_requests",
            schema: "provider",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_full_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                user_photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                tier_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                payment_status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                amount_usd = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                platform_fee_usd = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                decline_reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                decline_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                declined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_direct_booking_requests", x => x.id);
                table.ForeignKey("fk_direct_booking_requests_doctor_profiles_doctor_id", x => x.doctor_id, "doctor_profiles", "id", "provider", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("fk_direct_booking_requests_service_tiers_tier_id", x => x.tier_id, "service_tiers", "id", "provider", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "quick_chat_requests",
            schema: "provider",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_first_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                user_last_initial = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                doctor_id = table.Column<Guid>(type: "uuid", nullable: true),
                tier_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                user_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                doctor_reply = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                messaging_thread_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                replied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                sla_deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_quick_chat_requests", x => x.id);
                table.ForeignKey("fk_quick_chat_requests_doctor_profiles_doctor_id", x => x.doctor_id, "doctor_profiles", "id", "provider", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("fk_quick_chat_requests_service_tiers_tier_id", x => x.tier_id, "service_tiers", "id", "provider", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "doctor_session_reports",
            schema: "provider",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_full_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                session_number = table.Column<int>(type: "integer", nullable: false),
                session_date = table.Column<DateTime>(type: "date", nullable: false),
                mood_before = table.Column<int>(type: "integer", nullable: false),
                mood_after = table.Column<int>(type: "integer", nullable: false),
                goals_completed = table.Column<int>(type: "integer", nullable: false),
                goals_total = table.Column<int>(type: "integer", nullable: false),
                progress_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                techniques_used = table.Column<string[]>(type: "text[]", nullable: false),
                next_steps = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_session_reports", x => x.id);
                table.ForeignKey("fk_session_reports_direct_booking_requests_booking_id", x => x.booking_id, "direct_booking_requests", "id", "provider", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("fk_session_reports_doctor_profiles_doctor_id", x => x.doctor_id, "doctor_profiles", "id", "provider", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("ix_doctor_dashboard_accounts_email", "doctor_dashboard_accounts", "email", "auth", unique: true);
        migrationBuilder.CreateIndex("ix_doctor_refresh_tokens_doctor_account_id_expires_at", "doctor_refresh_tokens", new[] { "doctor_account_id", "expires_at" }, "auth");
        migrationBuilder.CreateIndex("ix_doctor_refresh_tokens_token_hash", "doctor_refresh_tokens", "token_hash", "auth", unique: true);
        migrationBuilder.CreateIndex("ix_doctor_profiles_doctor_account_id", "doctor_profiles", "doctor_account_id", "provider", unique: true);
        migrationBuilder.CreateIndex("ix_doctor_profiles_is_available", "doctor_profiles", "is_available", "provider");
        migrationBuilder.CreateIndex("ix_doctor_educations_doctor_id", "doctor_educations", "doctor_id", "provider");
        migrationBuilder.CreateIndex("ix_service_tiers_is_active_price_usd", "service_tiers", new[] { "is_active", "price_usd" }, "provider");
        migrationBuilder.CreateIndex("ix_service_tiers_name", "service_tiers", "name", "provider", unique: true);
        migrationBuilder.CreateIndex("ix_quick_chat_requests_status_sla_deadline", "quick_chat_requests", new[] { "status", "sla_deadline" }, "provider");
        migrationBuilder.CreateIndex("ix_quick_chat_requests_doctor_id_status_created_at", "quick_chat_requests", new[] { "doctor_id", "status", "created_at" }, "provider");
        migrationBuilder.CreateIndex("ix_quick_chat_requests_tier_id_status", "quick_chat_requests", new[] { "tier_id", "status" }, "provider");
        migrationBuilder.CreateIndex("ix_direct_booking_requests_doctor_id_status_created_at", "direct_booking_requests", new[] { "doctor_id", "status", "created_at" }, "provider");
        migrationBuilder.CreateIndex("ix_direct_booking_requests_user_id_doctor_id", "direct_booking_requests", new[] { "user_id", "doctor_id" }, "provider");
        migrationBuilder.CreateIndex("ix_direct_booking_requests_tier_id", "direct_booking_requests", "tier_id", "provider");
        migrationBuilder.CreateIndex("ix_session_reports_booking_id_session_number", "doctor_session_reports", new[] { "booking_id", "session_number" }, "provider", unique: true);
        migrationBuilder.CreateIndex("ix_session_reports_doctor_id_session_date", "doctor_session_reports", new[] { "doctor_id", "session_date" }, "provider");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "doctor_session_reports", schema: "provider");
        migrationBuilder.DropTable(name: "quick_chat_requests", schema: "provider");
        migrationBuilder.DropTable(name: "doctor_educations", schema: "provider");
        migrationBuilder.DropTable(name: "doctor_refresh_tokens", schema: "auth");
        migrationBuilder.DropTable(name: "direct_booking_requests", schema: "provider");
        migrationBuilder.DropTable(name: "doctor_profiles", schema: "provider");
        migrationBuilder.DropTable(name: "service_tiers", schema: "provider");
        migrationBuilder.DropTable(name: "doctor_dashboard_accounts", schema: "auth");
    }
}
