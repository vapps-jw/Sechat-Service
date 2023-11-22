using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sechat.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserReferralPass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferralPass",
                table: "UserProfiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferralPass",
                table: "UserProfiles");
        }
    }
}
