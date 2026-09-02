using Microsoft.AspNetCore.Mvc;
using AydinMensucat.StokTakip.Models;
using Microsoft.EntityFrameworkCore;

namespace AydinMensucat.StokTakip.Controllers
{
    // Kullanıcı girişi ve çıkışını yönetir. Yetkilendirme burada tutulan
    // Session bilgilerine ("KullaniciAdi", "UserRole") dayanır; diğer tüm
    // controller'lardaki IsIzleyici() kontrolü bu Session verisini okur.
    public class AccountController : Controller
    {
        private readonly StokTakipContext _context;

        public AccountController(StokTakipContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        // Giriş formunu gösterir.
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        // Kullanıcı adı/şifre eşleşirse Session'a kullanıcı bilgisini yazıp
        // Ana Menü'ye yönlendirir. Bu action, hem normal giriş ekranından hem de
        // sol menüdeki "Hesap Değiştir" modalından çağrılır — ikisi de aynı akışı kullanır.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string kullaniciAdi, string sifre)
        {
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi && k.Sifre == sifre);

            if (kullanici != null)
            {
                // Oturum bilgilerini Session'a kaydediyoruz
                HttpContext.Session.SetString("KullaniciAdi", kullanici.KullaniciAdi);
                HttpContext.Session.SetString("UserRole", kullanici.Rol);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Hatalı kullanıcı adı veya şifre!");
            return View();
        }

        // Çıkış Yap
        // Session'daki tüm bilgileri temizleyip giriş ekranına döner.
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}