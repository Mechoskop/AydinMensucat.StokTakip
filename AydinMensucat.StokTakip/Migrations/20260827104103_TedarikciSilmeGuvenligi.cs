using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AydinMensucat.StokTakip.Migrations
{
    /// <inheritdoc />
    public partial class TedarikciSilmeGuvenligi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urunler_Tedarikciler_TedarikciId",
                table: "Urunler");

            migrationBuilder.AddForeignKey(
                name: "FK_Urunler_Tedarikciler_TedarikciId",
                table: "Urunler",
                column: "TedarikciId",
                principalTable: "Tedarikciler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urunler_Tedarikciler_TedarikciId",
                table: "Urunler");

            migrationBuilder.AddForeignKey(
                name: "FK_Urunler_Tedarikciler_TedarikciId",
                table: "Urunler",
                column: "TedarikciId",
                principalTable: "Tedarikciler",
                principalColumn: "Id");
        }
    }
}
