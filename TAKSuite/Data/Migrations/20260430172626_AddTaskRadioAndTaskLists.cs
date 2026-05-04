using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskRadioAndTaskLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Durata",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RadioChannelId",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sottotitolo",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipologiaObiettivo",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ActionItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionItem_Tasks_TaskEntityId",
                        column: x => x.TaskEntityId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InfoItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfoItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfoItem_Tasks_TaskEntityId",
                        column: x => x.TaskEntityId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoteItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteItem_Tasks_TaskEntityId",
                        column: x => x.TaskEntityId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_RadioChannelId",
                table: "Tasks",
                column: "RadioChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionItem_TaskEntityId",
                table: "ActionItem",
                column: "TaskEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_InfoItem_TaskEntityId",
                table: "InfoItem",
                column: "TaskEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteItem_TaskEntityId",
                table: "NoteItem",
                column: "TaskEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_RadioChannels_RadioChannelId",
                table: "Tasks",
                column: "RadioChannelId",
                principalTable: "RadioChannels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_RadioChannels_RadioChannelId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "ActionItem");

            migrationBuilder.DropTable(
                name: "InfoItem");

            migrationBuilder.DropTable(
                name: "NoteItem");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_RadioChannelId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Durata",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RadioChannelId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Sottotitolo",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TipologiaObiettivo",
                table: "Tasks");
        }
    }
}
