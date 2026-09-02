# Aydın Mensucat - Depo ve Envanter Yönetim Sistemi

Aydın Mensucat bünyesindeki hammadde ve ekipman stoklarının departmanlar
arası dağılımını takip etmek amacıyla geliştirilmiş, web tabanlı bir
depo/envanter yönetim sistemidir. Staj kapsamında C# ve ASP.NET Core MVC
kullanılarak sıfırdan geliştirilmiştir.

## Özellikler

- Ürün ekleme, düzenleme, silme, arama/filtreleme ve sıralama
- Departmanlar arası ürün transferi
- Müşteriye stok çıkışı / sevkiyat takibi
- Tedarikçi ve müşteri cari kart yönetimi
- Kategori bazlı ürün sınıflandırması
- Tüm işlemlerin kaydedildiği stok hareket geçmişi (denetim izi)
- Aylık stok giriş grafiği (Chart.js)
- Excel'e (CSV) aktarma
- Rol bazlı yetkilendirme (Tam Yetkili / Salt Okunur)
- Oturum açıkken hızlı hesap değiştirme

## Kullanılan Teknolojiler

- **Backend:** ASP.NET Core MVC, C#
- **Veritabanı:** Microsoft SQL Server, Entity Framework Core (Code-First)
- **Frontend:** Razor View Engine, Bootstrap 5, Bootstrap Icons
- **Grafik:** Chart.js
- **Kimlik Doğrulama:** Session tabanlı, global Action Filter ile korunan erişim

## Kurulum

1. Repoyu klonlayın.
2. `appsettings.json` içindeki bağlantı dizesini kendi SQL Server ortamınıza göre düzenleyin.
3. Package Manager Console'da migration'ları uygulayın:# AydinMensucat.StokTakip
