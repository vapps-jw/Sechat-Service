using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sechat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Calendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Calendars_UserProfileId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "AllDay",
                table: "CalendarEvent");

            migrationBuilder.AlterColumn<string>(
                name: "Start",
                table: "CalendarEvent",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "End",
                table: "CalendarEvent",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "CalendarEvent",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_UserProfileId",
                table: "Calendars",
                column: "UserProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Calendars_UserProfileId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "CalendarEvent");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Start",
                table: "CalendarEvent",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "End",
                table: "CalendarEvent",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AllDay",
                table: "CalendarEvent",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_UserProfileId",
                table: "Calendars",
                column: "UserProfileId");
        }
    }
}
