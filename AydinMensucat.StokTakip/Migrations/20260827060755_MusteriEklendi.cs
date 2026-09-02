using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AydinMensucat.StokTakip.Migrations
{
    /// <inheritdoc />
    public partial class MusteriEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MusteriId",
                table: "StokHareketleri",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Musteriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YetkiliKisi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musteriler", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketleri_MusteriId",
                table: "StokHareketleri",
                column: "MusteriId");

            migrationBuilder.AddForeignKey(
                name: "FK_StokHareketleri_Musteriler_MusteriId",
                table: "StokHareketleri",
                column: "MusteriId",
                principalTable: "Musteriler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StokHareketleri_Musteriler_MusteriId",
                table: "StokHareketleri");

            migrationBuilder.DropTable(
                name: "Musteriler");

            migrationBuilder.DropIndex(
                name: "IX_StokHareketleri_MusteriId",
                table: "StokHareketleri");

            migrationBuilder.DropColumn(
                name: "MusteriId",
                table: "StokHareketleri");
        }
    }
}
