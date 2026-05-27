using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Data.Migrations
{
    /// <inheritdoc />
    public partial class TakConnection2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "TakSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "TakSettings",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "TakSettings",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "TakSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AiSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraSystemPrompt = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScoreConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MissionTAKSuiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxBivacco = table.Column<int>(type: "int", nullable: false),
                    MaxAiutoCartografico = table.Column<int>(type: "int", nullable: false),
                    MaxOperatoreNonDichiarato = table.Column<int>(type: "int", nullable: false),
                    MaxMarcaturaAsgAssente = table.Column<int>(type: "int", nullable: false),
                    MaxFasciaNonEsposta = table.Column<int>(type: "int", nullable: false),
                    MaxOperatoreSqualificato = table.Column<int>(type: "int", nullable: false),
                    MaxInterferenzaArbitrale = table.Column<int>(type: "int", nullable: false),
                    MaxComportamentoAntisportivo = table.Column<int>(type: "int", nullable: false),
                    MaxAsgOverJoule = table.Column<int>(type: "int", nullable: false),
                    MaxSaccoRifiuti = table.Column<int>(type: "int", nullable: false),
                    MaxDifensoriEliminati = table.Column<int>(type: "int", nullable: false),
                    MaxRibelliColpiti = table.Column<int>(type: "int", nullable: false),
                    MaxCiviliColpiti = table.Column<int>(type: "int", nullable: false),
                    MostraFasiENulle = table.Column<bool>(type: "bit", nullable: false),
                    FasiEJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreConfigs_Tasks_TaskEntityId",
                        column: x => x.TaskEntityId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskScoreEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentificativoPattuglia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipologia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurataMinuti = table.Column<int>(type: "int", nullable: true),
                    InizioObj = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FineObj = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FuoriFinestra = table.Column<bool>(type: "bit", nullable: false),
                    PunteggioMinimo = table.Column<bool>(type: "bit", nullable: false),
                    Bivacco = table.Column<int>(type: "int", nullable: false),
                    AiutoCartografico = table.Column<int>(type: "int", nullable: false),
                    OperatoreNonDichiarato = table.Column<int>(type: "int", nullable: false),
                    MarcaturaAsgAssente = table.Column<int>(type: "int", nullable: false),
                    FasciaNonEsposta = table.Column<int>(type: "int", nullable: false),
                    OperatoreSqualificato = table.Column<int>(type: "int", nullable: false),
                    InterferenzaArbitrale = table.Column<int>(type: "int", nullable: false),
                    ComportamentoAntisportivo = table.Column<int>(type: "int", nullable: false),
                    AsgOverJoule = table.Column<int>(type: "int", nullable: false),
                    SaccoRifiuti = table.Column<int>(type: "int", nullable: false),
                    DifensoriEliminati = table.Column<int>(type: "int", nullable: false),
                    RibelliColpiti = table.Column<int>(type: "int", nullable: false),
                    CiviliColpiti = table.Column<int>(type: "int", nullable: false),
                    FasiEJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MostraFasiENulle = table.Column<bool>(type: "bit", nullable: false),
                    NotePattugliaIncursori = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoteArbitro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskScoreEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskScoreEntries_Tasks_TaskEntityId",
                        column: x => x.TaskEntityId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScoreConfigs_TaskEntityId",
                table: "ScoreConfigs",
                column: "TaskEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScoreEntries_TaskEntityId",
                table: "TaskScoreEntries",
                column: "TaskEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiSettings");

            migrationBuilder.DropTable(
                name: "ScoreConfigs");

            migrationBuilder.DropTable(
                name: "TaskScoreEntries");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "TakSettings");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "TakSettings");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "TakSettings");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "TakSettings");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "AspNetUsers");
        }
    }
}
