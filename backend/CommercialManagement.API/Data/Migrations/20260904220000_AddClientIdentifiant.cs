using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommercialManagement.API.Data.Migrations;

public partial class AddClientIdentifiant : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Identifiant",
            table: "Clients",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true);

        migrationBuilder.Sql("UPDATE dbo.Clients SET Identifiant = CONCAT('CLI-', Id) WHERE Identifiant IS NULL");

        migrationBuilder.AlterColumn<string>(
            name: "Identifiant",
            table: "Clients",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Clients_Identifiant",
            table: "Clients",
            column: "Identifiant",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Clients_Identifiant",
            table: "Clients");

        migrationBuilder.DropColumn(
            name: "Identifiant",
            table: "Clients");
    }
}
