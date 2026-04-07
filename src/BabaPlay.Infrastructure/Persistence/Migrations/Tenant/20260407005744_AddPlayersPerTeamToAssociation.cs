using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabaPlay.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddPlayersPerTeamToAssociation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayersPerTeam",
                table: "Associations",
                type: "int",
                nullable: false,
                defaultValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayersPerTeam",
                table: "Associations");
        }
    }
}
