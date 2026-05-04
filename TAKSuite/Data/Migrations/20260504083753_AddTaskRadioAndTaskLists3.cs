using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskRadioAndTaskLists3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TaskStringItems_TaskEntityId",
                table: "TaskStringItems",
                column: "TaskEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskStringItems_Tasks_TaskEntityId",
                table: "TaskStringItems",
                column: "TaskEntityId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskStringItems_Tasks_TaskEntityId",
                table: "TaskStringItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskStringItems_TaskEntityId",
                table: "TaskStringItems");
        }
    }
}
