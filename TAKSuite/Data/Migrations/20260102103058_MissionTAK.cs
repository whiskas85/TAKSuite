using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class MissionTAK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MissionSuiteId",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MissionTAKSuiteId",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MissionSuite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MissionUids = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionSuite", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_MissionSuiteId",
                table: "Teams",
                column: "MissionSuiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_MissionTAKSuiteId",
                table: "Tasks",
                column: "MissionTAKSuiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_MissionSuite_MissionTAKSuiteId",
                table: "Tasks",
                column: "MissionTAKSuiteId",
                principalTable: "MissionSuite",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_MissionSuite_MissionSuiteId",
                table: "Teams",
                column: "MissionSuiteId",
                principalTable: "MissionSuite",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_MissionSuite_MissionTAKSuiteId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_MissionSuite_MissionSuiteId",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "MissionSuite");

            migrationBuilder.DropIndex(
                name: "IX_Teams_MissionSuiteId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_MissionTAKSuiteId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "MissionSuiteId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "MissionTAKSuiteId",
                table: "Tasks");
        }
    }
}
