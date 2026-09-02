using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AydinMensucat.StokTakip.Migrations
{
    /// <inheritdoc />
    public partial class StokHareketiAciklamaEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "StokHareketleri",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "StokHareketleri");
        }
    }
}
