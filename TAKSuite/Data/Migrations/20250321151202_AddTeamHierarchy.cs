using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentTeamId",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ParentTeamId",
                table: "Teams",
                column: "ParentTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Teams_ParentTeamId",
                table: "Teams",
                column: "ParentTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Teams_ParentTeamId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_ParentTeamId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ParentTeamId",
                table: "Teams");
        }
    }
}
