using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    public partial class AddTaskScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoreConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxBivacco = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAiutoCartografico = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxOperatoreNonDichiarato = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxMarcaturaAsgAssente = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxFasciaNonEsposta = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxOperatoreSqualificato = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxInterferenzaArbitrale = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxComportamentoAntisportivo = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAsgOverJoule = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxSaccoRifiuti = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxDifensoriEliminati = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxCiviliColpiti = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FasiEJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]")
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
                    IdentificativoPattuglia = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    Tipologia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurataMinuti = table.Column<int>(type: "int", nullable: true),
                    InizioObj = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FineObj = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FuoriFinestra = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PunteggioMinimo = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Bivacco = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AiutoCartografico = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OperatoreNonDichiarato = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MarcaturaAsgAssente = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FasciaNonEsposta = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OperatoreSqualificato = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    InterferenzaArbitrale = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ComportamentoAntisportivo = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AsgOverJoule = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SaccoRifiuti = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DifensoriEliminati = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CiviliColpiti = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FasiEJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TaskScoreEntries");
            migrationBuilder.DropTable(name: "ScoreConfigs");
        }
    }
}
