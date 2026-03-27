using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RemovePositionSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Positions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
