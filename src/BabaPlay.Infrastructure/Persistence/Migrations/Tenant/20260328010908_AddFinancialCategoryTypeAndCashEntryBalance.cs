using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddFinancialCategoryTypeAndCashEntryBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBalance",
                table: "CashEntries",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_CashEntries_EntryDate",
                table: "CashEntries",
                column: "EntryDate");

            migrationBuilder.Sql(@"
WITH OrderedEntries AS (
    SELECT
        ce.Id,
        SUM(CASE WHEN c.[Type] = 1 THEN -ABS(ce.Amount) ELSE ABS(ce.Amount) END)
            OVER (ORDER BY ce.EntryDate, ce.CreatedAt, ce.Id ROWS UNBOUNDED PRECEDING) AS RunningBalance
    FROM CashEntries ce
    INNER JOIN Categories c ON c.Id = ce.CategoryId
)
UPDATE ce
SET ce.CurrentBalance = oe.RunningBalance
FROM CashEntries ce
INNER JOIN OrderedEntries oe ON oe.Id = ce.Id;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CashEntries_EntryDate",
                table: "CashEntries");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CurrentBalance",
                table: "CashEntries");
        }
    }
}
