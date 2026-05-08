using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionDatesAndTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "MissionsTakSuite",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime",
                table: "MissionsTakSuite",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime",
                table: "MissionsTakSuite",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionsTakSuite_TeamId",
                table: "MissionsTakSuite",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_MissionsTakSuite_Teams_TeamId",
                table: "MissionsTakSuite",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MissionsTakSuite_Teams_TeamId",
                table: "MissionsTakSuite");

            migrationBuilder.DropIndex(
                name: "IX_MissionsTakSuite_TeamId",
                table: "MissionsTakSuite");

            migrationBuilder.DropColumn(name: "TeamId", table: "MissionsTakSuite");
            migrationBuilder.DropColumn(name: "StartDateTime", table: "MissionsTakSuite");
            migrationBuilder.DropColumn(name: "EndDateTime", table: "MissionsTakSuite");
        }
    }
}
