using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;

#nullable disable

namespace LIOSCare.DoctorDashboard.Infrastructure.Migrations;

[DbContext(typeof(DoctorPortalDbContext))]
[Migration("202605170001_AddPasswordResetTokens")]
public partial class AddPasswordResetTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "password_reset_tokens",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                doctor_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_password_reset_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_password_reset_tokens_doctor_dashboard_accounts_doctor_account_id",
                    column: x => x.doctor_account_id,
                    principalSchema: "auth",
                    principalTable: "doctor_dashboard_accounts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_token_hash",
            schema: "auth",
            table: "password_reset_tokens",
            column: "token_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_doctor_account_id",
            schema: "auth",
            table: "password_reset_tokens",
            column: "doctor_account_id");

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_expires_at",
            schema: "auth",
            table: "password_reset_tokens",
            column: "expires_at");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "password_reset_tokens", schema: "auth");
    }
}
