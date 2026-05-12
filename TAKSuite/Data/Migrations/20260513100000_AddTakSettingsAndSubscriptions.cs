using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    public partial class AddTakSettingsAndSubscriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TakSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Callsign = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientUid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TakServerIp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HttpsPort = table.Column<int>(type: "int", nullable: false),
                    StreamingPort = table.Column<int>(type: "int", nullable: false),
                    EnrollmentPort = table.Column<int>(type: "int", nullable: false),
                    EnrollUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CertificatePassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CertificateP12 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TakSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TakSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MissionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscribedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TakSubscriptions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TakSettings");
            migrationBuilder.DropTable(name: "TakSubscriptions");
        }
    }
}
