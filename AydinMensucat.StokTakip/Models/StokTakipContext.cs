using Microsoft.EntityFrameworkCore;

namespace AydinMensucat.StokTakip.Models
{
    // Entity Framework Core'un veritabanı bağlantı noktası (DbContext).
    // Her DbSet<T>, veritabanındaki bir tabloyu temsil eder; ilişkiler (foreign key
    // davranışları) ise en altta OnModelCreating içinde tanımlanır.
    public class StokTakipContext : DbContext
    {
        public StokTakipContext(DbContextOptions<StokTakipContext> options) : base(options)
        {
        }

        // Fabrikadaki Raflarımız (Kategoriler Tablosu)
        public DbSet<Kategori> Kategoriler { get; set; }

        // Raflardaki Eşyalarımız (Ürünler Tablosu)
        public DbSet<Urun> Urunler { get; set; }

        // Stok Hareket Geçmişi Tablosu
        public DbSet<AydinMensucat.StokTakip.Models.StokHareketi> StokHareketleri { get; set; }

        // Kullanıcı Tablosu (Bilgi_islem ve Misafir)
        public DbSet<Kullanici> Kullanicilar { get; set; }

        // Tedarikçiler Tablosu
        public DbSet<Tedarikci> Tedarikciler { get; set; }

        // Müşteriler Tablosu (Cari Kart)
        public DbSet<Musteri> Musteriler { get; set; }

        // Foreign key ilişkilerinin silme davranışlarını (DeleteBehavior) burada
        // özel olarak tanımlıyoruz. Varsayılan olarak EF Core, zorunlu (nullable
        // olmayan) ilişkilerde "Cascade" (bağlı kayıtları da sil) davranışını seçer —
        // bu bizim için tehlikeli olurdu, o yüzden aşağıdaki iki ilişkiyi bilinçli
        // olarak "SetNull"a çeviriyoruz.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bir Müşteri silindiğinde, o müşteriye ait geçmiş StokHareketi kayıtları
            // SİLİNMEZ — sadece o kayıtların MusteriId alanı null'a düşer. Böylece
            // "hangi üründen kaç adet satıldı" gibi geçmiş veriler kaybolmaz.
            modelBuilder.Entity<StokHareketi>()
                .HasOne(sh => sh.Musteri)
                .WithMany(m => m.StokHareketleri)
                .HasForeignKey(sh => sh.MusteriId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bir Tedarikçi silindiğinde, ondan alınan ürünler SİLİNMEZ — sadece
            // o ürünlerin TedarikciId alanı null'a düşer (ürün stokta kalmaya devam eder,
            // sadece "hangi firmadan alındı" bilgisi boşalır).
            modelBuilder.Entity<Urun>()
                .HasOne(u => u.Tedarikci)
                .WithMany(t => t.Urunler)
                .HasForeignKey(u => u.TedarikciId)
                .OnDelete(DeleteBehavior.SetNull);

            // NOT: Urun -> Kategori ilişkisi burada özel olarak tanımlanmadı, çünkü
            // Urun.KategoriId alanı ZORUNLU (nullable değil). Bu yüzden EF Core
            // varsayılan olarak bu ilişkide "Cascade" davranışını kullanır — yani
            // bir Kategori silinirse, ona bağlı TÜM ürünler de otomatik silinir.
            // Bu riski kod seviyesinde değil, KategoriController.DeleteConfirmed
            // içinde (kategoride ürün varsa silmeyi baştan engelleyerek) kapatıyoruz.
        }
    }
}