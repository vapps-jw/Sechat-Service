using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sechat.Data.Migrations
{
    /// <inheritdoc />
    public partial class CalendarEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvent_Calendars_CalendarId",
                table: "CalendarEvent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarEvent",
                table: "CalendarEvent");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "CalendarEvent");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CalendarEvent");

            migrationBuilder.DropColumn(
                name: "End",
                table: "CalendarEvent");

            migrationBuilder.DropColumn(
                name: "IsAllDay",
                table: "CalendarEvent");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "CalendarEvent");

            migrationBuilder.RenameTable(
                name: "CalendarEvent",
                newName: "CalendarEvents");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "CalendarEvents",
                newName: "Data");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvent_CalendarId",
                table: "CalendarEvents",
                newName: "IX_CalendarEvents_CalendarId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Remind = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reminders = table.Column<int>(type: "integer", nullable: false),
                    Reminded = table.Column<int>(type: "integer", nullable: false),
                    CalendarEventId = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_CalendarEvents_CalendarEventId",
                        column: x => x.CalendarEventId,
                        principalTable: "CalendarEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_CalendarEventId",
                table: "Reminders",
                column: "CalendarEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_Remind",
                table: "Reminders",
                column: "Remind");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Calendars_CalendarId",
                table: "CalendarEvents",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Calendars_CalendarId",
                table: "CalendarEvents");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents");

            migrationBuilder.RenameTable(
                name: "CalendarEvents",
                newName: "CalendarEvent");

            migrationBuilder.RenameColumn(
                name: "Data",
                table: "CalendarEvent",
                newName: "Start");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvents_CalendarId",
                table: "CalendarEvent",
                newName: "IX_CalendarEvent_CalendarId");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "CalendarEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CalendarEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "End",
                table: "CalendarEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllDay",
                table: "CalendarEvent",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CalendarEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarEvent",
                table: "CalendarEvent",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvent_Calendars_CalendarId",
                table: "CalendarEvent",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
