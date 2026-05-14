using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIconColorToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorClass",
                table: "FoodCategories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "FoodCategories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorClass",
                table: "FoodCategories");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "FoodCategories");
        }
    }
}
