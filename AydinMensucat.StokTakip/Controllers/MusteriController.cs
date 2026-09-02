using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AydinMensucat.StokTakip.Models;
using Microsoft.AspNetCore.Http;

// Müşteri (ürün satılan/sevk edilen firma) kayıtlarının CRUD işlemlerini yönetir.
// Tedarikçi'den farkı: Müşteri, Ürün tablosuna değil StokHareketi tablosuna bağlıdır
// (bkz. Urun/Transfer action'ındaki "Müşteriye Çıkış" senaryosu) — çünkü bir ürünün
// stoğu birden fazla farklı müşteriye bölünerek gönderilebilir, tek bir müşteriye
// sabitlenemez.
public class MusteriController : Controller
{
    private readonly StokTakipContext _context;

    public MusteriController(StokTakipContext context)
    {
        _context = context;
    }

    // Müşteri Listesi
    public async Task<IActionResult> Index()
    {
        return View(await _context.Musteriler.ToListAsync());
    }

    // Müşteri Detayı + Hareket Geçmişi
    // "Include" ile StokHareketleri koleksiyonu da birlikte çekiliyor, böylece
    // detay sayfasında bu müşteriye geçmişte yapılan tüm sevkiyatlar listelenebiliyor.
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var musteri = await _context.Musteriler
            .Include(m => m.StokHareketleri)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (musteri == null) return NotFound();

        return View(musteri);
    }

    // Yeni Müşteri Ekleme (Get)
    public IActionResult Create()
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        return View();
    }

    // Yeni Müşteri Ekleme (Post)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Musteri musteri)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        // Navigation property'nin (StokHareketleri) formu patlatmasını engelliyoruz
        // — bu alan formdan gelmez, sadece ilişkiyi temsil eder.
        ModelState.Remove("StokHareketleri");

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
                _context.Add(musteri);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kritik Veritabanı Hatası: " + ex.Message);
            }
        }
        return View(musteri);
    }

    // Müşteri Düzenleme (Get)
    public async Task<IActionResult> Edit(int? id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        if (id == null) return NotFound();

        var musteri = await _context.Musteriler.FindAsync(id);
        if (musteri == null) return NotFound();

        return View(musteri);
    }

    // Müşteri Düzenleme (Post)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Musteri musteri)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        if (id != musteri.Id) return NotFound();

        ModelState.Remove("StokHareketleri");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(musteri);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kritik Veritabanı Hatası: " + ex.Message);
            }
        }
        return View(musteri);
    }

    // Müşteri Silme (Get - onay ekranı)
    public async Task<IActionResult> Delete(int? id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        if (id == null) return NotFound();

        var musteri = await _context.Musteriler.FirstOrDefaultAsync(m => m.Id == id);
        if (musteri == null) return NotFound();

        return View(musteri);
    }

    // Müşteri Silme (Post)
    // Not: Bu müşteriye ait geçmiş StokHareketleri kayıtları silinmez — sadece
    // ilgili kayıtların MusteriId alanı null'a düşer (OnModelCreating'deki
    // DeleteBehavior.SetNull kuralı sayesinde). Böylece geçmiş sevkiyat verisi kaybolmaz.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        var musteri = await _context.Musteriler.FindAsync(id);
        if (musteri != null)
        {
            _context.Musteriler.Remove(musteri);
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