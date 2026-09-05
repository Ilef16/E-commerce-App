using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommercialManagement.API.Data.Migrations;

public partial class AddProduitPhotoUrl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PhotoUrl",
            table: "Produits",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PhotoUrl", table: "Produits");
    }
}
