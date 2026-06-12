using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHaven.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteProjectRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentModerations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    ModeratorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentModerations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentModerations_Users_ModeratorUserId",
                        column: x => x.ModeratorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    StoredFileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileAssets_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FosterAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    FosterProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FosterAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FosterAssignments_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FosterAssignments_FosterProfiles_FosterProfileId",
                        column: x => x.FosterProfileId,
                        principalTable: "FosterProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedAnimalSearchFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Species = table.Column<string>(type: "text", nullable: true),
                    Breed = table.Column<string>(type: "text", nullable: true),
                    Size = table.Column<int>(type: "integer", nullable: true),
                    EnergyLevel = table.Column<int>(type: "integer", nullable: true),
                    SpecialNeedsOnly = table.Column<bool>(type: "boolean", nullable: false),
                    City = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    RadiusKm = table.Column<double>(type: "double precision", nullable: true),
                    LastCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedAnimalSearchFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedAnimalSearchFilters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FosterReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FosterAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BehaviorNotes = table.Column<string>(type: "text", nullable: false),
                    ProgressNotes = table.Column<string>(type: "text", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FosterReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FosterReports_FosterAssignments_FosterAssignmentId",
                        column: x => x.FosterAssignmentId,
                        principalTable: "FosterAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentModerations_ModeratorUserId",
                table: "ContentModerations",
                column: "ModeratorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_UploadedByUserId",
                table: "FileAssets",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FosterAssignments_AnimalId",
                table: "FosterAssignments",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_FosterAssignments_FosterProfileId",
                table: "FosterAssignments",
                column: "FosterProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FosterReports_FosterAssignmentId",
                table: "FosterReports",
                column: "FosterAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedAnimalSearchFilters_UserId",
                table: "SavedAnimalSearchFilters",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentModerations");

            migrationBuilder.DropTable(
                name: "FileAssets");

            migrationBuilder.DropTable(
                name: "FosterReports");

            migrationBuilder.DropTable(
                name: "SavedAnimalSearchFilters");

            migrationBuilder.DropTable(
                name: "FosterAssignments");
        }
    }
}
