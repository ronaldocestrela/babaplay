using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantGeolocationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AssociationLatitude",
                table: "Tenants",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AssociationLongitude",
                table: "Tenants",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckinRadiusMeters",
                table: "Tenants",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssociationLatitude",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "AssociationLongitude",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CheckinRadiusMeters",
                table: "Tenants");
        }
    }
}
