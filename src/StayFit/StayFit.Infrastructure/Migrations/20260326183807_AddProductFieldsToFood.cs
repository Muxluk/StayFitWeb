using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductFieldsToFood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodLogs_Foods_FoodId",
                table: "FoodLogs");

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Foods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Foods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Foods",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Foods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Foods",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Foods");

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
