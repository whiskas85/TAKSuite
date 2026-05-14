using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class JoinPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MissionPhotoJoinConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    RadiusMeters = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionPhotoJoinConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionPhotoJoinConfigs_MissionsTakSuite_MissionId",
                        column: x => x.MissionId,
                        principalTable: "MissionsTakSuite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CotPriorityRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhotoJoinConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    TypeContains = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallSignContains = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExcludedTypesCsv = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotPriorityRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotPriorityRules_MissionPhotoJoinConfigs_PhotoJoinConfigId",
                        column: x => x.PhotoJoinConfigId,
                        principalTable: "MissionPhotoJoinConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CotPriorityRules_PhotoJoinConfigId",
                table: "CotPriorityRules",
                column: "PhotoJoinConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionPhotoJoinConfigs_MissionId",
                table: "MissionPhotoJoinConfigs",
                column: "MissionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CotPriorityRules");

            migrationBuilder.DropTable(
                name: "MissionPhotoJoinConfigs");
        }
    }
}
