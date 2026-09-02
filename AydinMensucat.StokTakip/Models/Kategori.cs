namespace AydinMensucat.StokTakip.Models
{
    // Ürünlerin sınıflandırıldığı ana gruplar (örn. Bilgisayar & Donanım,
    // Hammadde & Malzeme). Her ürün tam olarak bir kategoriye bağlıdır.
    public class Kategori
    {
        public int Id { get; set; }
        public string KategoriAdi { get; set; }
        public string? Aciklama { get; set; }

        // İlişki: Bir kategorinin altında (IT, Kumaş, Ofis) birden fazla ürün olabilir
        public virtual ICollection<Urun> Urunler { get; set; } = new List<Urun>();
    }
}