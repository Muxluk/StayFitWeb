using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailBroadcastSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AdminId", table: "EmailBroadcasts");
            migrationBuilder.DropColumn(name: "Body", table: "EmailBroadcasts");
            migrationBuilder.DropColumn(name: "Audience", table: "EmailBroadcasts");

            migrationBuilder.AddColumn<int>(
                name: "AdminUserId",
                table: "EmailBroadcasts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HtmlBody",
                table: "EmailBroadcasts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "EmailBroadcasts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_EmailBroadcasts_AdminUserId",
                table: "EmailBroadcasts",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailBroadcasts_SentAt",
                table: "EmailBroadcasts",
                column: "SentAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_EmailBroadcasts_AdminUserId", table: "EmailBroadcasts");
            migrationBuilder.DropIndex(name: "IX_EmailBroadcasts_SentAt", table: "EmailBroadcasts");

            migrationBuilder.DropColumn(name: "AdminUserId", table: "EmailBroadcasts");
            migrationBuilder.DropColumn(name: "HtmlBody", table: "EmailBroadcasts");
            migrationBuilder.DropColumn(name: "Status", table: "EmailBroadcasts");

            migrationBuilder.AddColumn<string>(name: "AdminId", table: "EmailBroadcasts", type: "text", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "Body", table: "EmailBroadcasts", type: "text", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "Audience", table: "EmailBroadcasts", type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "");
        }
    }
}
