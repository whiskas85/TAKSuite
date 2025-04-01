using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class TaskHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskHierarchy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskHierarchy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskHierarchy_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskHierarchy_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogs_TeamId",
                table: "TaskLogs",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHierarchy_TaskId",
                table: "TaskHierarchy",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHierarchy_TeamId",
                table: "TaskHierarchy",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLogs_Teams_TeamId",
                table: "TaskLogs",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskLogs_Teams_TeamId",
                table: "TaskLogs");

            migrationBuilder.DropTable(
                name: "TaskHierarchy");

            migrationBuilder.DropIndex(
                name: "IX_TaskLogs_TeamId",
                table: "TaskLogs");
        }
    }
}
