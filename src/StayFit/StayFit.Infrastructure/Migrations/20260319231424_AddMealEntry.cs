using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StayFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMealEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MealEntryId",
                table: "FoodLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MealEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodLogs_MealEntryId",
                table: "FoodLogs",
                column: "MealEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodLogs_MealEntries_MealEntryId",
                table: "FoodLogs",
                column: "MealEntryId",
                principalTable: "MealEntries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodLogs_MealEntries_MealEntryId",
                table: "FoodLogs");

            migrationBuilder.DropTable(
                name: "MealEntries");

            migrationBuilder.DropIndex(
                name: "IX_FoodLogs_MealEntryId",
                table: "FoodLogs");

            migrationBuilder.DropColumn(
                name: "MealEntryId",
                table: "FoodLogs");
        }
    }
}
