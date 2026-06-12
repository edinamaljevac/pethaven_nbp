using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHaven.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShelterCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ShelterProfiles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ShelterProfiles",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ShelterProfiles");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ShelterProfiles");
        }
    }
}
