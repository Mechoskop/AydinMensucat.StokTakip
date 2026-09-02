using System.ComponentModel.DataAnnotations;

namespace AydinMensucat.StokTakip.Models
{
    // Hammadde/ekipman alınan firmaları temsil eder. Musteri'nin tam tersi
    // yönde çalışır: burada ilişki doğrudan Urun tablosuna kurulu, çünkü bir
    // ürünün tedarikçisi genelde sabittir (o ürünü hep aynı firmadan alırsın).
    public class Tedarikci
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket adı boş bırakılamaz!")]
        public string SirketAdi { get; set; }

        public string YetkiliKisi { get; set; }
        public string Telefon { get; set; }
        public string Adres { get; set; }

        // Bu tedarikçiden alınmış ürünler (bkz. Urun.TedarikciId)
        public ICollection<Urun> Urunler { get; set; }
    }
}