using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StayFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationFlagsAndSupportFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdminNotified",
                table: "Foods",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Додаємо колонку IsAdminNotified до вже існуючої таблиці SupportTickets
            migrationBuilder.AddColumn<bool>(
                name: "IsAdminNotified",
                table: "SupportTickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            
            // Оновлюємо довжину статусу, якщо вона змінилася в моделі
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SupportTickets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportTicketReplies");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "IsAdminNotified",
                table: "Foods");
        }
    }
}
