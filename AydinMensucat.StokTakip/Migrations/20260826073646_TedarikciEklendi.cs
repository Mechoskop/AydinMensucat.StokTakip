using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AydinMensucat.StokTakip.Migrations
{
    /// <inheritdoc />
    public partial class TedarikciEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TedarikciId",
                table: "Urunler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tedarikciler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SirketAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YetkiliKisi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tedarikciler", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_TedarikciId",
                table: "Urunler",
                column: "TedarikciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Urunler_Tedarikciler_TedarikciId",
                table: "Urunler",
                column: "TedarikciId",
                principalTable: "Tedarikciler",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urunler_Tedarikciler_TedarikciId",
                table: "Urunler");

            migrationBuilder.DropTable(
                name: "Tedarikciler");

            migrationBuilder.DropIndex(
                name: "IX_Urunler_TedarikciId",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "TedarikciId",
                table: "Urunler");
        }
    }
}
