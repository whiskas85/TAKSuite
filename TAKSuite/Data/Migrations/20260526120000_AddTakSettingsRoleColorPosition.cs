using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Data.Migrations
{
    public partial class AddTakSettingsRoleColorPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "TakSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Team Member");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "TakSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "TakSettings",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "TakSettings",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Role",      table: "TakSettings");
            migrationBuilder.DropColumn(name: "Color",     table: "TakSettings");
            migrationBuilder.DropColumn(name: "Latitude",  table: "TakSettings");
            migrationBuilder.DropColumn(name: "Longitude", table: "TakSettings");
        }
    }
}
