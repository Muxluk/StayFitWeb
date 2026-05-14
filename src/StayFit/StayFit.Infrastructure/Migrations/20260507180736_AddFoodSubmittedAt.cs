using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodSubmittedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Foods",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Foods");
        }
    }
}
