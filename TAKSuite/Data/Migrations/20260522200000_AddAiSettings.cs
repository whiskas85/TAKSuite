using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Data.Migrations
{
    public partial class AddAiSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiSettings",
                columns: table => new
                {
                    Id             = table.Column<int>(type: "int", nullable: false),
                    IsEnabled      = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Provider       = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Endpoint       = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApiKey         = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Model          = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    ExtraSystemPrompt = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSettings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AiSettings");
        }
    }
}
