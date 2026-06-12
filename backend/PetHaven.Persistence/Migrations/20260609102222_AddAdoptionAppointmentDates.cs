using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHaven.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdoptionAppointmentDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HomeVisitScheduledAt",
                table: "AdoptionApplications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InterviewScheduledAt",
                table: "AdoptionApplications",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeVisitScheduledAt",
                table: "AdoptionApplications");

            migrationBuilder.DropColumn(
                name: "InterviewScheduledAt",
                table: "AdoptionApplications");
        }
    }
}
