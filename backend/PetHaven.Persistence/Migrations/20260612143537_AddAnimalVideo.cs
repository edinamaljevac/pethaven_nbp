using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHaven.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Animals",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Animals");
        }
    }
}
