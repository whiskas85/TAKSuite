using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Migrations
{
    /// <inheritdoc />
    public partial class FixRegCodeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Aggiungi una nuova colonna temporanea con Guid
            migrationBuilder.AddColumn<Guid>(
                name: "NewId",
                table: "RegistrationCodes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            // 2. Copia i dati dalla vecchia colonna alla nuova (se necessario)
            migrationBuilder.Sql("UPDATE RegistrationCodes SET NewId = NEWID()");

            // 3. Rimuovi i vincoli sulla vecchia colonna
            migrationBuilder.DropPrimaryKey(name: "PK_RegistrationCodes", table: "RegistrationCodes");

            // 4. Elimina la colonna `Id` vecchia
            migrationBuilder.DropColumn(name: "Id", table: "RegistrationCodes");

            // 5. Rinomina la nuova colonna come `Id`
            migrationBuilder.RenameColumn(
                name: "NewId",
                table: "RegistrationCodes",
                newName: "Id");

            // 6. Aggiungi nuovamente la chiave primaria
            migrationBuilder.AddPrimaryKey(name: "PK_RegistrationCodes", table: "RegistrationCodes", column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ripristina l'ID come int se necessario
            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "RegistrationCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE RegistrationCodes SET OldId = 1"); // Modifica se necessario

            migrationBuilder.DropPrimaryKey(name: "PK_RegistrationCodes", table: "RegistrationCodes");
            migrationBuilder.DropColumn(name: "Id", table: "RegistrationCodes");

            migrationBuilder.RenameColumn(
                name: "OldId",
                table: "RegistrationCodes",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_RegistrationCodes", table: "RegistrationCodes", column: "Id");
        }
    }

}