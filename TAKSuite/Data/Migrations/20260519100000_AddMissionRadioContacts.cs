using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionRadioContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MissionRadioContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RadioChannelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BackupRadioChannelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionRadioContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionRadioContacts_MissionsTakSuite_MissionId",
                        column: x => x.MissionId,
                        principalTable: "MissionsTakSuite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionRadioContacts_RadioChannels_RadioChannelId",
                        column: x => x.RadioChannelId,
                        principalTable: "RadioChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionRadioContacts_RadioChannels_BackupRadioChannelId",
                        column: x => x.BackupRadioChannelId,
                        principalTable: "RadioChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MissionRadioContacts_MissionId",
                table: "MissionRadioContacts",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionRadioContacts_RadioChannelId",
                table: "MissionRadioContacts",
                column: "RadioChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionRadioContacts_BackupRadioChannelId",
                table: "MissionRadioContacts",
                column: "BackupRadioChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MissionRadioContacts");
        }
    }
}
