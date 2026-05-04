using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class MissionTAK_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_MissionSuite_MissionTAKSuiteId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_MissionSuite_MissionSuiteId",
                table: "Teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissionSuite",
                table: "MissionSuite");

            migrationBuilder.RenameTable(
                name: "MissionSuite",
                newName: "MissionsTakSuite");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissionsTakSuite",
                table: "MissionsTakSuite",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_MissionsTakSuite_MissionTAKSuiteId",
                table: "Tasks",
                column: "MissionTAKSuiteId",
                principalTable: "MissionsTakSuite",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_MissionsTakSuite_MissionSuiteId",
                table: "Teams",
                column: "MissionSuiteId",
                principalTable: "MissionsTakSuite",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_MissionsTakSuite_MissionTAKSuiteId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_MissionsTakSuite_MissionSuiteId",
                table: "Teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissionsTakSuite",
                table: "MissionsTakSuite");

            migrationBuilder.RenameTable(
                name: "MissionsTakSuite",
                newName: "MissionSuite");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissionSuite",
                table: "MissionSuite",
                column: "Id");

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
    }
}
