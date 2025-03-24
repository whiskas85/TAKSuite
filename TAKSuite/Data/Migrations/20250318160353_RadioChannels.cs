using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class RadioChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamRadioChannel_RadioChannel_RadioChannelId1",
                table: "TeamRadioChannel");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamRadioChannel_Teams_TeamId",
                table: "TeamRadioChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeamRadioChannel",
                table: "TeamRadioChannel");

            migrationBuilder.DropIndex(
                name: "IX_TeamRadioChannel_RadioChannelId1",
                table: "TeamRadioChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RadioChannel",
                table: "RadioChannel");

            migrationBuilder.DropColumn(
                name: "RadioChannelId1",
                table: "TeamRadioChannel");

            migrationBuilder.RenameTable(
                name: "TeamRadioChannel",
                newName: "TeamRadioChannels");

            migrationBuilder.RenameTable(
                name: "RadioChannel",
                newName: "RadioChannels");

            migrationBuilder.RenameIndex(
                name: "IX_TeamRadioChannel_TeamId",
                table: "TeamRadioChannels",
                newName: "IX_TeamRadioChannels_TeamId");

            migrationBuilder.AddColumn<Guid>(
                name: "BackupRadioChannelId",
                table: "TeamRadioChannels",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<double>(
                name: "Frequency",
                table: "RadioChannels",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "RadioChannels",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeamRadioChannels",
                table: "TeamRadioChannels",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RadioChannels",
                table: "RadioChannels",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRadioChannels_BackupRadioChannelId",
                table: "TeamRadioChannels",
                column: "BackupRadioChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRadioChannels_RadioChannelId",
                table: "TeamRadioChannels",
                column: "RadioChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRadioChannels_RadioChannels_BackupRadioChannelId",
                table: "TeamRadioChannels",
                column: "BackupRadioChannelId",
                principalTable: "RadioChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRadioChannels_RadioChannels_RadioChannelId",
                table: "TeamRadioChannels",
                column: "RadioChannelId",
                principalTable: "RadioChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRadioChannels_Teams_TeamId",
                table: "TeamRadioChannels",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamRadioChannels_RadioChannels_BackupRadioChannelId",
                table: "TeamRadioChannels");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamRadioChannels_RadioChannels_RadioChannelId",
                table: "TeamRadioChannels");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamRadioChannels_Teams_TeamId",
                table: "TeamRadioChannels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeamRadioChannels",
                table: "TeamRadioChannels");

            migrationBuilder.DropIndex(
                name: "IX_TeamRadioChannels_BackupRadioChannelId",
                table: "TeamRadioChannels");

            migrationBuilder.DropIndex(
                name: "IX_TeamRadioChannels_RadioChannelId",
                table: "TeamRadioChannels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RadioChannels",
                table: "RadioChannels");

            migrationBuilder.DropColumn(
                name: "BackupRadioChannelId",
                table: "TeamRadioChannels");

            migrationBuilder.RenameTable(
                name: "TeamRadioChannels",
                newName: "TeamRadioChannel");

            migrationBuilder.RenameTable(
                name: "RadioChannels",
                newName: "RadioChannel");

            migrationBuilder.RenameIndex(
                name: "IX_TeamRadioChannels_TeamId",
                table: "TeamRadioChannel",
                newName: "IX_TeamRadioChannel_TeamId");

            migrationBuilder.AddColumn<string>(
                name: "RadioChannelId1",
                table: "TeamRadioChannel",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Frequency",
                table: "RadioChannel",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "RadioChannel",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeamRadioChannel",
                table: "TeamRadioChannel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RadioChannel",
                table: "RadioChannel",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRadioChannel_RadioChannelId1",
                table: "TeamRadioChannel",
                column: "RadioChannelId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRadioChannel_RadioChannel_RadioChannelId1",
                table: "TeamRadioChannel",
                column: "RadioChannelId1",
                principalTable: "RadioChannel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRadioChannel_Teams_TeamId",
                table: "TeamRadioChannel",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
