using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddCheckInSessionDailyUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtDateUtc",
                table: "CheckInSessions",
                type: "date",
                nullable: true,
                computedColumnSql: "CAST([StartedAt] AS DATE)",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckInSessions_StartedAtDateUtc",
                table: "CheckInSessions",
                column: "StartedAtDateUtc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CheckInSessions_StartedAtDateUtc",
                table: "CheckInSessions");

            migrationBuilder.DropColumn(
                name: "StartedAtDateUtc",
                table: "CheckInSessions");
        }
    }
}
