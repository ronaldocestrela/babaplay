using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddAssociationStatute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Academics",
                table: "Associations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                schema: "Academics",
                table: "Associations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Statute",
                schema: "Academics",
                table: "Associations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Academics",
                table: "Associations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Academics",
                table: "Associations");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                schema: "Academics",
                table: "Associations");

            migrationBuilder.DropColumn(
                name: "Statute",
                schema: "Academics",
                table: "Associations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "Academics",
                table: "Associations");
        }
    }
}
