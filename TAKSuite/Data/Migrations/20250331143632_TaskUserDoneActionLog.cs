using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class TaskUserDoneActionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "TaskLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogs_UserId",
                table: "TaskLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLogs_UsersAtak_UserId",
                table: "TaskLogs",
                column: "UserId",
                principalTable: "UsersAtak",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskLogs_UsersAtak_UserId",
                table: "TaskLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskLogs_UserId",
                table: "TaskLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TaskLogs");
        }
    }
}
