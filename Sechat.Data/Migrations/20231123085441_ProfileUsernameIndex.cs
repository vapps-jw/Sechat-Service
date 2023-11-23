using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sechat.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProfileUsernameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserName",
                table: "UserProfiles",
                column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_UserName",
                table: "UserProfiles");
        }
    }
}
