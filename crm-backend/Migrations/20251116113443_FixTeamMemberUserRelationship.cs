using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crm_backend.Migrations
{
    /// <inheritdoc />
    public partial class FixTeamMemberUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Users_UserId1",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_UserId1",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "TeamMembers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "TeamMembers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_UserId1",
                table: "TeamMembers",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Users_UserId1",
                table: "TeamMembers",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
