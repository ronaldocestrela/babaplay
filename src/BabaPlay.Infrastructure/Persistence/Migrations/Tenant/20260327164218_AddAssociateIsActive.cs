using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAssociateIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Associates",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Associates");
        }
    }
}
