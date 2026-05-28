using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    public partial class AddProtoCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProtoStaleMinutes",
                table: "TakSettings",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "ProtoDeleteMinutes",
                table: "TakSettings",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.CreateTable(
                name: "CachedCoTEntries",
                columns: table => new
                {
                    Uid       = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Callsign  = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CotType   = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lat       = table.Column<double>(type: "float",         nullable: false),
                    Lon       = table.Column<double>(type: "float",         nullable: false),
                    Hae       = table.Column<double>(type: "float",         nullable: false),
                    Team      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawXml    = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "datetime2",   nullable: false),
                    LastSeen  = table.Column<DateTime>(type: "datetime2",   nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedCoTEntries", x => x.Uid);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CachedCoTEntries");
            migrationBuilder.DropColumn(name: "ProtoStaleMinutes",  table: "TakSettings");
            migrationBuilder.DropColumn(name: "ProtoDeleteMinutes", table: "TakSettings");
        }
    }
}
