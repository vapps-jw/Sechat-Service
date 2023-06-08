using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sechat.Data.Migrations
{
    /// <inheritdoc />
    public partial class EncryptedRoomFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EncryptedByUser",
                table: "Rooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedByUser",
                table: "Rooms");
        }
    }
}
