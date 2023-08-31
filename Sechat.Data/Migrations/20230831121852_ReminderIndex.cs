using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sechat.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReminderIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reminders_Reminded",
                table: "Reminders",
                column: "Reminded");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminders_Reminded",
                table: "Reminders");
        }
    }
}
