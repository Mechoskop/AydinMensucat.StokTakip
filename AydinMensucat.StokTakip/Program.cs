using Microsoft.EntityFrameworkCore;
using AydinMensucat.StokTakip.Models; // Model namespace'imiz

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AydinMensucat.StokTakip.Models.StokTakipContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
// GirisKontrolFiltresi, giriş yapmamış kullanıcıların herhangi bir sayfaya
// (Login ekranı hariç) erişmesini engelleyen global bir güvenlik filtresidir.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AydinMensucat.StokTakip.Filters.GirisKontrolFiltresi>();
});

// Adım 6 için Session (Oturum) Servisini Ekliyoruz
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika işlem yapılmazsa oturum kapansın
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Uygulama ayağa kalkarken örnek kategorileri ve senin kullanıcılarını veritabanına ekleme (Seeding)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StokTakipContext>();

    // 1. Kategori tablosu boşsa örnek kategorileri ekle (Ürün ekleme ekranı boş kalmasın diye)
    if (!context.Kategoriler.Any())
    {
        context.Kategoriler.AddRange(
            new Kategori { KategoriAdi = "Bilgisayar & Donanım" },
            new Kategori { KategoriAdi = "Hammadde & Malzeme" },
            new Kategori { KategoriAdi = "Sarf Malzeme" }
        );
        context.SaveChanges();
    }

    // 2. Senin belirlediğin kullanıcılar tablosu boşsa ekle
    if (!context.Kullanicilar.Any())
    {
        context.Kullanicilar.AddRange(
            new Kullanici { KullaniciAdi = "bilgi_islem", Sifre = "1234", Rol = "IT" },
            new Kullanici { KullaniciAdi = "misafir", Sifre = "12345", Rol = "Izleyici" }
        );
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Standart statik dosyalar için

app.UseRouting();

// Adım 6 için Session Middleware'ini Ekliyoruz (UseRouting ile UseAuthorization arasına)
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}") // Uygulama direkt Login ekranıyla açılacak şekilde ayarladık!
    .WithStaticAssets();

app.Run();