namespace AydinMensucat.StokTakip.Models
{
    // Sisteme giriş yapan hesapları temsil eder. Rol'e göre iki seviye yetki var:
    // "IT" tam yetkili (ekleme/düzenleme/silme/transfer yapabilir),
    // "Izleyici" salt okunur (sadece görüntüleyebilir).
    // Not: Sifre alanı şu an düz metin (plaintext) olarak tutuluyor — staj projesi
    // kapsamında bilinçli olarak basit bırakıldı, üretim ortamına taşınırsa
    // hashlenmesi (örn. BCrypt) gerekir.
    public class Kullanici
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
        public string Rol { get; set; } // "IT" (Tam Yetkili) veya "Izleyici" (Salt Okunur)
    }
}