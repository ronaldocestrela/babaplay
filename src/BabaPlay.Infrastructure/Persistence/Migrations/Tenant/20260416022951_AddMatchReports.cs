using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddMatchReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchReports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalizedByUserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchReportGames",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MatchReportId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GameNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchReportGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchReportGames_MatchReports_MatchReportId",
                        column: x => x.MatchReportId,
                        principalTable: "MatchReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchReportPlayerStats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MatchReportGameId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssociateId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Goals = table.Column<int>(type: "int", nullable: false),
                    Assists = table.Column<int>(type: "int", nullable: false),
                    YellowCards = table.Column<int>(type: "int", nullable: false),
                    RedCards = table.Column<int>(type: "int", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchReportPlayerStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchReportPlayerStats_MatchReportGames_MatchReportGameId",
                        column: x => x.MatchReportGameId,
                        principalTable: "MatchReportGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchReportGames_MatchReportId_GameNumber",
                table: "MatchReportGames",
                columns: new[] { "MatchReportId", "GameNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchReportPlayerStats_AssociateId",
                table: "MatchReportPlayerStats",
                column: "AssociateId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchReportPlayerStats_MatchReportGameId_AssociateId",
                table: "MatchReportPlayerStats",
                columns: new[] { "MatchReportGameId", "AssociateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchReports_SessionId",
                table: "MatchReports",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchReportPlayerStats");

            migrationBuilder.DropTable(
                name: "MatchReportGames");

            migrationBuilder.DropTable(
                name: "MatchReports");
        }
    }
}
