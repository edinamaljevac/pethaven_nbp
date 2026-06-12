using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHaven.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerApplicationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VolunteerApplications",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "VolunteerApplications");
        }
    }
}
