using AydinMensucat.StokTakip.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AydinMensucat.StokTakip.Controllers
{
    // Giriş yapıldıktan sonra karşılanan "Ana Menü" sayfasını ve genel hata
    // sayfasını yönetir. Sistemin ana iş mantığı (ürün, stok vb.) burada değil,
    // UrunController ve diğer controller'larda.
    public class HomeController : Controller
    {
        // Ana Menü: saat/takvim/hava durumu widget'larının ve hızlı erişim
        // kısayollarının bulunduğu karşılama sayfası. Giriş yapan kullanıcının
        // adını görebilmek için ViewBag üzerinden Session'dan çekiyoruz.
        public IActionResult Index()
        {
            ViewBag.KullaniciAdi = HttpContext.Session.GetString("KullaniciAdi") ?? "Misafir";
            return View();
        }

        // Beklenmeyen bir hata oluştuğunda (ör. bir sayfa çökerse) yönlendirilen
        // genel hata sayfası. Önbelleğe alınmaması için ResponseCache ile devre dışı
        // bırakıldı — yoksa tarayıcı eski bir hata sayfasını göstermeye devam edebilir.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}