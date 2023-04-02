using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sechat.Data.Migrations
{
    /// <inheritdoc />
    public partial class messageviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageViewers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageViewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageViewers_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Created",
                table: "Messages",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_MessageViewers_MessageId",
                table: "MessageViewers",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageViewers_UserId",
                table: "MessageViewers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageViewers");

            migrationBuilder.DropIndex(
                name: "IX_Messages_Created",
                table: "Messages");
        }
    }
}
