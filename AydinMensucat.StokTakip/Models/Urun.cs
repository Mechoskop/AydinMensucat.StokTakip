using System.ComponentModel.DataAnnotations;

namespace AydinMensucat.StokTakip.Models
{
    // Sistemin temel varlığı: tek bir "stok kartı" satırını temsil eder.
    // Not: Aynı ürün (örn. "Lenovo Laptop") farklı departmanlarda ayrı ayrı
    // satırlar olarak tutulur — yani "ürün" ile "o üründen bir departmandaki
    // stok" aynı tablo içinde iç içe geçmiş durumda. Departmanlar arası transferde
    // (UrunController.Transfer) bu yüzden yeni satır oluşturulabiliyor ya da
    // var olan satırın miktarı güncelleniyor.
    public class Urun
    {
        public int Id { get; set; }

        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }

        [Display(Name = "Marka")]
        public string Marka { get; set; }

        [Display(Name = "Model")]
        public string Model { get; set; }

        [Display(Name = "Özellikler")]
        public string Ozellikler { get; set; }

        [Display(Name = "Stok Miktarı")]
        public int StokMiktari { get; set; }

        [Display(Name = "Birim")]
        public string Birim { get; set; }

        // Zorunlu ilişki: her ürünün bir kategorisi olmak zorunda (nullable değil).
        // Bu yüzden bir Kategori silinirse EF Core varsayılan olarak bu ürünleri de
        // siler (cascade) — KategoriController bunu, kategoride ürün varsa silmeyi
        // baştan engelleyerek kontrol altına alıyor.
        [Display(Name = "Kategori")]
        public int KategoriId { get; set; }
        public Kategori Kategori { get; set; }

        // Departman, ayrı bir tablo değil düz metin olarak tutuluyor
        // (örn. "IT", "Üretim", "Muhasebe"). Küçük/orta ölçek için yeterli;
        // departman sayısı çok artarsa ayrı bir Departman tablosuna geçmek gerekebilir.
        [Display(Name = "Departman")]
        public string Departman { get; set; }

        // Opsiyonel ilişki: bir ürünün tedarikçisi olmayabilir (örn. iç üretim).
        // Tedarikçi silinirse bu alan null'a düşer, ürün silinmez (bkz. StokTakipContext).
        public int? TedarikciId { get; set; }
        public Tedarikci Tedarikci { get; set; }
    }
}