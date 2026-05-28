using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TAKSuite.Data;

#nullable disable

namespace TAKSuite.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260528140000_AddCachedCoTMission")]
    public partial class AddCachedCoTMission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MissionName",
                table: "CachedCoTEntries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CachedCoTEntries_MissionName",
                table: "CachedCoTEntries",
                column: "MissionName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CachedCoTEntries_MissionName",
                table: "CachedCoTEntries");

            migrationBuilder.DropColumn(
                name: "MissionName",
                table: "CachedCoTEntries");
        }
    }
}
