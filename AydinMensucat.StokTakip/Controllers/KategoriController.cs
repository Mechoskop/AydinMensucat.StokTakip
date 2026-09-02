using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AydinMensucat.StokTakip.Models;

// Ürün kategorilerinin (Bilgisayar & Donanım, Hammadde & Malzeme vb.) CRUD işlemlerini yönetir.
public class KategoriController : Controller
{
    private readonly StokTakipContext _context;

    public KategoriController(StokTakipContext context)
    {
        _context = context;
    }

    // GET: Kategori
    // Her kategorinin yanında kaç farklı ürün içerdiğini gösterebilmek için
    // Urunler koleksiyonu da birlikte çekiliyor.
    public async Task<IActionResult> Index()
    {
        var kategoriler = await _context.Kategoriler.Include(k => k.Urunler).ToListAsync();
        return View(kategoriler);
    }

    // GET: Kategori/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var kategori = await _context.Kategoriler
            .Include(k => k.Urunler)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (kategori == null) return NotFound();

        return View(kategori);
    }

    // GET: Kategori/Create
    public IActionResult Create()
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        return View();
    }

    // POST: Kategori/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,KategoriAdi,Aciklama")] Kategori kategori)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        if (ModelState.IsValid)
        {
            _context.Add(kategori);
            await _context.SaveChangesAsync();
            TempData["ToastMesaj"] = "Kategori başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(kategori);
    }

    // GET: Kategori/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        if (id == null) return NotFound();

        var kategori = await _context.Kategoriler.FindAsync(id);
        if (kategori == null) return NotFound();
        return View(kategori);
    }

    // POST: Kategori/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,KategoriAdi,Aciklama")] Kategori kategori)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        if (id != kategori.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(kategori);
                await _context.SaveChangesAsync();
                TempData["ToastMesaj"] = "Kategori başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KategoriExists(kategori.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(kategori);
    }

    // GET: Kategori/Delete/5
    // Bu kategoriye bağlı ürünleri de çekiyoruz ki View'de "X ürün var, silinemez" uyarısı gösterebilelim.
    public async Task<IActionResult> Delete(int? id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));
        if (id == null) return NotFound();

        var kategori = await _context.Kategoriler
            .Include(k => k.Urunler)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (kategori == null) return NotFound();

        return View(kategori);
    }

    // POST: Kategori/Delete/5
    // GÜVENLİK: Urun.KategoriId alanı zorunlu (nullable değil) olduğu için, Entity Framework
    // varsayılan olarak bu kategoriyi silerken ona bağlı TÜM ürünleri de otomatik silebilir
    // (cascade delete). Bunu engellemek için, kategoride hâlâ ürün varsa silme işlemini
    // burada durduruyoruz — önce ürünlerin başka bir kategoriye taşınması gerekiyor.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        var kategori = await _context.Kategoriler
            .Include(k => k.Urunler)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kategori == null) return RedirectToAction(nameof(Index));

        if (kategori.Urunler.Any())
        {
            TempData["ToastMesaj"] = $"Bu kategoride {kategori.Urunler.Count} ürün olduğu için silinemedi. Önce ürünleri başka bir kategoriye taşıyın.";
            TempData["ToastTip"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        _context.Kategoriler.Remove(kategori);
        await _context.SaveChangesAsync();
        TempData["ToastMesaj"] = "Kategori silindi.";
        return RedirectToAction(nameof(Index));
    }

    private bool KategoriExists(int? id)
    {
        return _context.Kategoriler.Any(e => e.Id == id);
    }

    // Yetki Kontrolü için Yardımcı Metot (İzleyici yazma yapamaz)
    private bool IsIzleyici()
    {
        string rol = HttpContext.Session.GetString("UserRole") ?? "";
        return rol == "Izleyici";
    }
}