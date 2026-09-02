using System.ComponentModel.DataAnnotations;

namespace AydinMensucat.StokTakip.Models
{
    // Ürün sevk edilen/satılan firmaları temsil eden cari kart.
    // Bilinçli olarak Urun tablosuna değil StokHareketi'ne bağlanır — çünkü bir
    // ürünün stoğu tek bir müşteriye sabitlenemez, farklı miktarlarda birden
    // fazla farklı müşteriye gönderilebilir (bkz. UrunController.Transfer).
    public class Musteri
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Firma/Müşteri adı boş bırakılamaz!")]
        [Display(Name = "Firma / Müşteri Adı")]
        public string MusteriAdi { get; set; }

        [Display(Name = "Yetkili Kişi")]
        public string? YetkiliKisi { get; set; }

        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        // Bu müşteriye ait geçmiş stok çıkışları/sevkiyatlar
        public ICollection<StokHareketi> StokHareketleri { get; set; } = new List<StokHareketi>();
    }
}