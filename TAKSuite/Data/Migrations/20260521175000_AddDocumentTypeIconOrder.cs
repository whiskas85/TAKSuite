using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTypeIconOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "DocumentTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "DocumentTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "DocumentTypes");
        }
    }
}
