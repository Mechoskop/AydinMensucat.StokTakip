namespace AydinMensucat.StokTakip.Models
{
    // Tüm sayfa başlıklarının (h2 + alt yazı) tek bir kalıptan render edilmesini
    // sağlayan basit bir "taşıyıcı" model. Bkz: Views/Shared/_SayfaBasligi.cshtml
    public class SayfaBasligiModel
    {
        // Başlığın düz (vurgusuz) kısmı. Örn: "Yeni "
        public string DuzMetin { get; set; }

        // Başlığın altın sarısı + italik vurgulanan kısmı. Örn: "Ürün Ekle"
        // Boş bırakılırsa hiç vurgu olmaz, başlık tamamen düz görünür.
        public string? VurguMetin { get; set; }

        // Başlığın altındaki küçük gri açıklama satırı. Boş bırakılırsa hiç gösterilmez.
        public string? AltBaslik { get; set; }

        // true ise başlık kırmızı (tehlike) renginde gösterilir, vurgu rengi devre dışı kalır.
        // Sadece "Sil" onay ekranları gibi yerlerde kullanılır.
        public bool Tehlike { get; set; } = false;
    }
}