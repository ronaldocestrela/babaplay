using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddFinancialCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashTransactions_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMonthlyFees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMonthlyFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMonthlyFees_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyFeePayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonthlyFeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false),
                    ReversedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyFeePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyFeePayments_PlayerMonthlyFees_MonthlyFeeId",
                        column: x => x.MonthlyFeeId,
                        principalTable: "PlayerMonthlyFees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_PlayerId",
                table: "CashTransactions",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_TenantId_OccurredOnUtc",
                table: "CashTransactions",
                columns: new[] { "TenantId", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_TenantId_PlayerId_OccurredOnUtc",
                table: "CashTransactions",
                columns: new[] { "TenantId", "PlayerId", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_TenantId_Type_OccurredOnUtc",
                table: "CashTransactions",
                columns: new[] { "TenantId", "Type", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyFeePayments_MonthlyFeeId",
                table: "MonthlyFeePayments",
                column: "MonthlyFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyFeePayments_TenantId_IsReversed_PaidAtUtc",
                table: "MonthlyFeePayments",
                columns: new[] { "TenantId", "IsReversed", "PaidAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyFeePayments_TenantId_MonthlyFeeId_PaidAtUtc",
                table: "MonthlyFeePayments",
                columns: new[] { "TenantId", "MonthlyFeeId", "PaidAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMonthlyFees_PlayerId",
                table: "PlayerMonthlyFees",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMonthlyFees_TenantId_PlayerId_Year_Month",
                table: "PlayerMonthlyFees",
                columns: new[] { "TenantId", "PlayerId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMonthlyFees_TenantId_Status_DueDateUtc",
                table: "PlayerMonthlyFees",
                columns: new[] { "TenantId", "Status", "DueDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMonthlyFees_TenantId_Year_Month",
                table: "PlayerMonthlyFees",
                columns: new[] { "TenantId", "Year", "Month" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashTransactions");

            migrationBuilder.DropTable(
                name: "MonthlyFeePayments");

            migrationBuilder.DropTable(
                name: "PlayerMonthlyFees");
        }
    }
}
