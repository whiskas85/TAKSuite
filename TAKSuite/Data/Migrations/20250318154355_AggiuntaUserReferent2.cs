using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class AggiuntaUserReferent2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReferentUserId",
                table: "Teams",
                newName: "TeamLeaderId");

            migrationBuilder.CreateTable(
                name: "RadioChannel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadioChannel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamRadioChannel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RadioChannelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RadioChannelId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BeginValidityPeriod = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndValidityPeriod = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamRadioChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamRadioChannel_RadioChannel_RadioChannelId1",
                        column: x => x.RadioChannelId1,
                        principalTable: "RadioChannel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamRadioChannel_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamRadioChannel_RadioChannelId1",
                table: "TeamRadioChannel",
                column: "RadioChannelId1");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRadioChannel_TeamId",
                table: "TeamRadioChannel",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamRadioChannel");

            migrationBuilder.DropTable(
                name: "RadioChannel");

            migrationBuilder.RenameColumn(
                name: "TeamLeaderId",
                table: "Teams",
                newName: "ReferentUserId");
        }
    }
}
