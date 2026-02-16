using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewCollounsInTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "Multitenancy",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "Multitenancy",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "Multitenancy",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "Multitenancy",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                schema: "Multitenancy",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "Multitenancy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "Multitenancy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "Multitenancy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "Multitenancy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                schema: "Multitenancy",
                table: "Tenants");
        }
    }
}
