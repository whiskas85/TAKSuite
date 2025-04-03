using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class TaskEnanchementPriority4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CardColor",
                table: "TaskPriorities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardColor",
                table: "TaskPriorities");
        }
    }
}
