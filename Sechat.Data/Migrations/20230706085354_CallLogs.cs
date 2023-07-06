using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sechat.Data.Migrations
{
    /// <inheritdoc />
    public partial class CallLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalleeName",
                table: "CallLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasViewed",
                table: "CallLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CalleeId",
                table: "CallLogs",
                column: "CalleeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CallLogs_CalleeId",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "CalleeName",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "WasViewed",
                table: "CallLogs");
        }
    }
}
