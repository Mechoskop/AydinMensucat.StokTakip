using System;
using System.ComponentModel.DataAnnotations;

namespace AydinMensucat.StokTakip.Models
{
    // Sistemdeki her türlü stok değişikliğinin kaydedildiği "denetim izi" (audit log)
    // tablosu: yeni ürün ekleme, departmanlar arası transfer, müşteriye çıkış,
    // stok azaltma (arıza/zayiat) ve ürün silme — hepsi buraya bir satır olarak düşer.
    // UrunAdi ve Kullanici alanlarının bilinçli olarak foreign key değil düz metin
    // (o anki bilginin "anlık görüntüsü") olarak tutulduğuna dikkat: böylece ürün
    // veya kullanıcı daha sonra silinse bile geçmiş kayıt bozulmadan kalır.
    public class StokHareketi
    {
        public int Id { get; set; }

        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }

        [Display(Name = "İşlem Türü")]
        public string IslemTuru { get; set; } // Örn: "Yeni Ürün Eklendi", "Stok Güncellendi", "Ürün Silindi"

        [Display(Name = "Adet")]
        public int Miktar { get; set; }

        [Display(Name = "İşlem Tarihi")]
        public DateTime Tarih { get; set; } = DateTime.Now;
        public string Departman { get; set; } // İşlemin yapıldığı veya ürünün ait olduğu departman
        public string? Aciklama { get; set; } // Örn. stok azaltmada "arızalı", "zayiat" gibi bir sebep
        public string? Kullanici { get; set; }

        // Opsiyonel ilişki: sadece "Müşteriye Çıkış" türündeki hareketlerde doludur.
        // Müşteri silinirse bu alan null'a düşer, hareket kaydı silinmez (bkz. StokTakipContext).
        [Display(Name = "Müşteri")]
        public int? MusteriId { get; set; }
        public Musteri? Musteri { get; set; }
    }
}