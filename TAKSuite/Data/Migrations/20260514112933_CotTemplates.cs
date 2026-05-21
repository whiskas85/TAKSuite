using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class CotTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Registra JoinPhoto come già applicata (le tabelle esistono nel DB ma non erano tracciate)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260513094607_JoinPhoto')
                BEGIN
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ('20260513094607_JoinPhoto', '10.0.7')
                END
            ");

            migrationBuilder.AddColumn<string>(
                name: "PoiLinksJson",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CotTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CotXml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconPreview = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotTemplates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CotTemplates");

            migrationBuilder.DropColumn(
                name: "PoiLinksJson",
                table: "Tasks");

            migrationBuilder.Sql(@"
                DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260513094607_JoinPhoto'
            ");
        }
    }
}
