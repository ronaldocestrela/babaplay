using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAssociateInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssociateInvitations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsSingleUse = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    InvitedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsesCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociateInvitations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssociateInvitations_Email",
                table: "AssociateInvitations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AssociateInvitations_ExpiresAt",
                table: "AssociateInvitations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AssociateInvitations_IsSingleUse_Email_ExpiresAt",
                table: "AssociateInvitations",
                columns: new[] { "IsSingleUse", "Email", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssociateInvitations_Token",
                table: "AssociateInvitations",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssociateInvitations");
        }
    }
}
