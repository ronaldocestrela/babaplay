using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddPlayerScoreSourceEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerScoreSourceEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppliedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerScoreSourceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerScoreSourceEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerScoreSourceEvents_PlayerId",
                table: "PlayerScoreSourceEvents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerScoreSourceEvents_TenantId_PlayerId",
                table: "PlayerScoreSourceEvents",
                columns: new[] { "TenantId", "PlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerScoreSourceEvents_TenantId_SourceEventId",
                table: "PlayerScoreSourceEvents",
                columns: new[] { "TenantId", "SourceEventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerScoreSourceEvents");
        }
    }
}
