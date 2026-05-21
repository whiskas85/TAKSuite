using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class MissionRadioContact : Migration
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
                        name: "FK_MissionRadioContacts_RadioChannels_BackupRadioChannelId",
                        column: x => x.BackupRadioChannelId,
                        principalTable: "RadioChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionRadioContacts_RadioChannels_RadioChannelId",
                        column: x => x.RadioChannelId,
                        principalTable: "RadioChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhoneContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cognome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Squadra = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneContacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MissionPhoneContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionPhoneContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionPhoneContacts_MissionsTakSuite_MissionId",
                        column: x => x.MissionId,
                        principalTable: "MissionsTakSuite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionPhoneContacts_PhoneContacts_PhoneContactId",
                        column: x => x.PhoneContactId,
                        principalTable: "PhoneContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MissionPhoneContacts_MissionId",
                table: "MissionPhoneContacts",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionPhoneContacts_PhoneContactId",
                table: "MissionPhoneContacts",
                column: "PhoneContactId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionRadioContacts_BackupRadioChannelId",
                table: "MissionRadioContacts",
                column: "BackupRadioChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionRadioContacts_MissionId",
                table: "MissionRadioContacts",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionRadioContacts_RadioChannelId",
                table: "MissionRadioContacts",
                column: "RadioChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MissionPhoneContacts");

            migrationBuilder.DropTable(
                name: "MissionRadioContacts");

            migrationBuilder.DropTable(
                name: "PhoneContacts");
        }
    }
}
