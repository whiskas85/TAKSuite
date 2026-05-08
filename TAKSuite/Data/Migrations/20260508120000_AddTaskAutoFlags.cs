using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAutoFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoScheduleTeam",
                table: "MissionsTakSuite",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoAssignTeam",
                table: "MissionsTakSuite",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoAcceptTask",
                table: "MissionsTakSuite",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AutoScheduleTeam", table: "MissionsTakSuite");
            migrationBuilder.DropColumn(name: "AutoAssignTeam", table: "MissionsTakSuite");
            migrationBuilder.DropColumn(name: "AutoAcceptTask", table: "MissionsTakSuite");
        }
    }
}
