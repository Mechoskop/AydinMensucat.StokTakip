using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AydinMensucat.StokTakip.Filters
{
    // Sisteme giriş yapılmadan hiçbir sayfaya (Login/Logout ekranı hariç) erişilememesini
    // sağlayan global bir güvenlik filtresi. Her controller'a tek tek "giriş yapılmış mı"
    // kontrolü eklemek yerine, bu filtre TÜM action çağrılarını otomatik olarak süzer.
    public class GirisKontrolFiltresi : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerAdi = context.RouteData.Values["controller"]?.ToString();

            // Login/Logout ekranlarına (AccountController) her zaman izin veriyoruz,
            // yoksa kullanıcı giriş ekranına bile ulaşamaz (sonsuz yönlendirme döngüsü oluşur).
            if (controllerAdi == "Account")
            {
                return;
            }

            var kullaniciAdi = context.HttpContext.Session.GetString("KullaniciAdi");

            // Session'da kullanıcı adı yoksa (hiç giriş yapılmamışsa), isteği durdurup
            // doğrudan Login ekranına yönlendiriyoruz. Artık URL'yi bilen biri,
            // giriş yapmadan hiçbir sayfaya erişemiyor.
            if (string.IsNullOrEmpty(kullaniciAdi))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Bu action'ın çalışmasından SONRA yapılacak bir işimiz yok,
            // ama arayüz (IActionFilter) bu metodu zorunlu kıldığı için boş bırakıyoruz.
        }
    }
}