using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class AddPoiNameToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PoiName",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PoiName",
                table: "Tasks");
        }
    }
}
