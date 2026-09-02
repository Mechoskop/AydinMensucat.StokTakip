using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AydinMensucat.StokTakip.Migrations
{
    /// <inheritdoc />
    public partial class StokHareketiEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StokHareketleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IslemTuru = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Miktar = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokHareketleri", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StokHareketleri");
        }
    }
}
