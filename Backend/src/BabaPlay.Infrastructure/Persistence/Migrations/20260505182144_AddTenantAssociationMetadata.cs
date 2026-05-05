using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAssociationMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Tenants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "Tenants",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Tenants",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "Tenants",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Tenants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Tenants",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Tenants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Tenants");
        }
    }
}
