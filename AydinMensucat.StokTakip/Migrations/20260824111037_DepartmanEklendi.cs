using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AydinMensucat.StokTakip.Migrations
{
    /// <inheritdoc />
    public partial class DepartmanEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Departman",
                table: "Urunler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Departman",
                table: "StokHareketleri",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Departman",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "Departman",
                table: "StokHareketleri");
        }
    }
}
