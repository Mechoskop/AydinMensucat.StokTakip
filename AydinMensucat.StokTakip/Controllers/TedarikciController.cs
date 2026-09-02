using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AydinMensucat.StokTakip.Models;
using Microsoft.AspNetCore.Http; // Session kullanımı için eklendi

// Tedarikçi (hammadde/ekipman alınan firma) kayıtlarının CRUD (ekleme, listeleme,
// düzenleme, silme) işlemlerini yönetir. Bir tedarikçi silindiğinde, ona bağlı
// ürünler silinmez — sadece o ürünlerin Tedarikçi bilgisi boşalır (bkz. StokTakipContext
// içindeki OnModelCreating, DeleteBehavior.SetNull kuralı).
public class TedarikciController : Controller
{
    private readonly StokTakipContext _context;

    public TedarikciController(StokTakipContext context)
    {
        _context = context;
    }

    // Tedarikçi Listesi
    public async Task<IActionResult> Index()
    {
        return View(await _context.Tedarikciler.ToListAsync());
    }

    // Yeni Tedarikçi Ekleme (Get)
    public IActionResult Create()
    {
        // GÜVENLİK: Misafir kullanıcı url'den girmeye çalışırsa listeye geri postala
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        return View();
    }

    // Yeni Tedarikçi Ekleme (Post)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tedarikci tedarikci)
    {
        // GÜVENLİK: Misafir kullanıcı post isteği atarsa engelle
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        // SİHİRLİ DOKUNUŞ: Urunler listesinin formu patlatmasını engelliyoruz
        // (Tedarikci modelindeki "Urunler" koleksiyonu formdan gelmediği için,
        // ModelState onu doğrulamaya çalışıp hataya düşmesin diye kaldırıyoruz)
        ModelState.Remove("Urunler");

        if (!ModelState.IsValid)
        {
            var hatalar = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            string hataMesaji = string.Join(" | ", hatalar);
            ModelState.AddModelError("", "VALIDATION HATASI: " + hataMesaji);
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(tedarikci);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kritik Veritabanı Hatası: " + ex.Message);
            }
        }
        return View(tedarikci);
    }

    // Tedarikçi Detayı + bu tedarikçiden alınan ürünler
    // "Include" ile Urunler koleksiyonu da birlikte çekiliyor, böylece detay
    // sayfasında "bu firmadan hangi ürünler alındı" listesi gösterilebiliyor.
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var tedarikci = await _context.Tedarikciler
            .Include(t => t.Urunler)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tedarikci == null) return NotFound();

        return View(tedarikci);
    }

    // Tedarikçi Düzenleme (Get)
    public async Task<IActionResult> Edit(int? id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        if (id == null) return NotFound();

        var tedarikci = await _context.Tedarikciler.FindAsync(id);
        if (tedarikci == null) return NotFound();

        return View(tedarikci);
    }

    // Tedarikçi Düzenleme (Post)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Tedarikci tedarikci)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        if (id != tedarikci.Id) return NotFound();

        ModelState.Remove("Urunler");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tedarikci);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kritik Veritabanı Hatası: " + ex.Message);
            }
        }
        return View(tedarikci);
    }

    // Tedarikçi Silme (Get - onay ekranı)
    public async Task<IActionResult> Delete(int? id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        if (id == null) return NotFound();

        var tedarikci = await _context.Tedarikciler.FirstOrDefaultAsync(t => t.Id == id);
        if (tedarikci == null) return NotFound();

        return View(tedarikci);
    }

    // Tedarikçi Silme (Post)
    // Not: Urunler tablosundaki TedarikciId alanı nullable olduğu ve DeleteBehavior.SetNull
    // ile yapılandırıldığı için, bu tedarikçiye bağlı ürünler silinmez — sadece
    // o ürünlerin Tedarikçi bilgisi boş kalır.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        var tedarikci = await _context.Tedarikciler.FindAsync(id);
        if (tedarikci != null)
        {
            _context.Tedarikciler.Remove(tedarikci);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // Yetki Kontrolü için Yardımcı Metot (İzleyici yazma yapamaz)
    private bool IsIzleyici()
    {
        string rol = HttpContext.Session.GetString("UserRole") ?? "";
        return rol == "Izleyici";
    }
}