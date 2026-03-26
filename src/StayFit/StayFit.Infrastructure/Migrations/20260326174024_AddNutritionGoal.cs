using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StayFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodLogs_Foods_FoodId",
                table: "FoodLogs");

            migrationBuilder.CreateTable(
                name: "NutritionGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CaloriesGoal = table.Column<float>(type: "real", nullable: false),
                    ProteinGoal = table.Column<float>(type: "real", nullable: false),
                    FatGoal = table.Column<float>(type: "real", nullable: false),
                    CarbsGoal = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionGoals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NutritionGoals_UserId",
                table: "NutritionGoals",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodLogs_Foods_FoodId",
                table: "FoodLogs",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodLogs_Foods_FoodId",
                table: "FoodLogs");

            migrationBuilder.DropTable(
                name: "NutritionGoals");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodLogs_Foods_FoodId",
                table: "FoodLogs",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
