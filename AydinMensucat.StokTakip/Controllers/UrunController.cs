using AydinMensucat.StokTakip.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

// Bu controller, sistemin en çok kullanılan modülünü yönetir: Ürünler.
// Ürün listeleme (arama/filtre/sıralama/sayfalama), ekleme, düzenleme, silme,
// departmanlar arası / müşteriye transfer, stok hareket geçmişi, grafik raporu
// ve Excel (CSV) dışa aktarma işlemlerinin hepsi burada toplanıyor.
public class UrunController : Controller
{
    private readonly StokTakipContext _context;

    public UrunController(StokTakipContext context)
    {
        _context = context;
    }

    // Yetki Kontrolü için Yardımcı Metot (İzleyici yazma yapamaz)
    // "Izleyici" rolündeki (misafir) kullanıcılar sadece görüntüleyebilir;
    // ekleme/düzenleme/silme/transfer action'larının başında bu metot çağrılarak
    // yetkisiz işlem yapılması engellenir.
    private bool IsIzleyici()
    {
        string rol = HttpContext.Session.GetString("UserRole") ?? "";
        return rol == "Izleyici";
    }

    // GET: URUNS
    // Ana ürün listesi sayfası. Tek bir action içinde birden fazla iş yapılıyor:
    // 1) Arama (ürün adı/marka/model), departman filtresi, "sadece stokta olanlar" filtresi
    // 2) Sütun bazlı sıralama (ürün adı / stok miktarı / departman)
    // 3) Üstteki 3 istatistik kartı için toplam sayılar (kategori, birim, departman dağılımı)
    // 4) Sayfalama (10'ar ürünlük sayfalar halinde listeleme)
    public async Task<IActionResult> Index(string searchString, string departmanFiltre, bool sadeceStoktaOlanlar = false, string sortOrder = null, int sayfa = 1)
    {
        // 🛠️ SİHİRLİ DOKUNUŞ: Veritabanındaki bozuk NULL kayıtları otomatik onarır
        try
        {
            await _context.Database.ExecuteSqlRawAsync("UPDATE Urunler SET StokMiktari = 0 WHERE StokMiktari IS NULL");
            await _context.Database.ExecuteSqlRawAsync("UPDATE Urunler SET KategoriId = (SELECT TOP 1 Id FROM Kategoriler) WHERE KategoriId IS NULL");
        }
        catch { /* Olası hataları yoksay ve devam et */ }

        var urunlerQuery = _context.Urunler.Include(u => u.Kategori).AsQueryable();

        // Arama filtresi (Ad, Marka, Model)
        if (!string.IsNullOrEmpty(searchString))
        {
            urunlerQuery = urunlerQuery.Where(s => s.UrunAdi.Contains(searchString) || s.Marka.Contains(searchString) || s.Model.Contains(searchString));
        }

        // Departman filtresi
        if (!string.IsNullOrEmpty(departmanFiltre))
        {
            urunlerQuery = urunlerQuery.Where(u => u.Departman == departmanFiltre);
        }

        // Sadece stokta olanları göster filtresi (varsayılan: tükenenler de gösterilir)
        if (sadeceStoktaOlanlar)
        {
            urunlerQuery = urunlerQuery.Where(u => u.StokMiktari > 0);
        }
        ViewBag.SadeceStoktaOlanlar = sadeceStoktaOlanlar;

        // Sıralama (tıklanan sütun başlığına göre)
        // ViewData["...Siralama"] alanları, View tarafında başlığa tıklanınca gidilecek
        // "bir sonraki" sıralama değerini taşır (yani bir nevi "toggle" - aç/kapa mantığı).
        ViewData["AdSiralama"] = String.IsNullOrEmpty(sortOrder) ? "ad_desc" : "";
        ViewData["StokSiralama"] = sortOrder == "stok_asc" ? "stok_desc" : "stok_asc";
        ViewData["DepartmanSiralama"] = sortOrder == "departman_asc" ? "departman_desc" : "departman_asc";
        ViewData["CurrentSort"] = sortOrder;

        urunlerQuery = sortOrder switch
        {
            "ad_desc" => urunlerQuery.OrderByDescending(u => u.UrunAdi),
            "stok_asc" => urunlerQuery.OrderBy(u => u.StokMiktari),
            "stok_desc" => urunlerQuery.OrderByDescending(u => u.StokMiktari),
            "departman_asc" => urunlerQuery.OrderBy(u => u.Departman),
            "departman_desc" => urunlerQuery.OrderByDescending(u => u.Departman),
            _ => urunlerQuery.OrderBy(u => u.UrunAdi), // Varsayılan: ürün adına göre A-Z
        };

        // Yeşil kart için birim bazlı toplam stok miktarları
        ViewData["ToplamAdet"] = await _context.Urunler.Where(u => u.Birim == "adet").SumAsync(u => u.StokMiktari);
        ViewData["ToplamKg"] = await _context.Urunler.Where(u => u.Birim == "kg").SumAsync(u => u.StokMiktari);
        ViewData["ToplamMetre"] = await _context.Urunler.Where(u => u.Birim == "metre").SumAsync(u => u.StokMiktari);
        ViewData["ToplamRulo"] = await _context.Urunler.Where(u => u.Birim == "rulo").SumAsync(u => u.StokMiktari);

        // Sağdaki kart için departman bazlı ürün çeşidi dağılımı — dinamik, en çok üründen aza sıralı ilk 3 + Diğer
        // Departman isimleri sabit yazılmıyor: hangi departmanda kaç farklı ürün varsa
        // ona göre büyükten küçüğe sıralanıp ilk 3'ü kartta, kalanı "Diğer" tooltip'inde gösteriliyor.
        var departmanSayilariRaw = await _context.Urunler
            .Where(u => u.Departman != null)
            .GroupBy(u => u.Departman)
            .Select(g => new { Departman = g.Key, Sayi = g.Count() })
            .OrderByDescending(g => g.Sayi)
            .ToListAsync();

        var departmanSayilari = departmanSayilariRaw
            .Select(x => new KeyValuePair<string, int>(x.Departman, x.Sayi))
            .ToList();

        var ilkUcDepartman = departmanSayilari.Take(3).ToList();
        var digerDepartmanlar = departmanSayilari.Skip(3).ToList();

        ViewData["Top3Departmanlar"] = ilkUcDepartman;
        ViewData["DepDiger"] = digerDepartmanlar.Sum(d => d.Value);

        // Rolü ve departman listesini arayüze aktarıyoruz
        // (View tarafında "Düzenle/Sil" gibi butonları role göre gizlemek için kullanılıyor)
        ViewBag.UserRole = HttpContext.Session.GetString("UserRole") ?? "Izleyici";
        ViewBag.KullaniciAdi = HttpContext.Session.GetString("KullaniciAdi") ?? "Misafir";

        // Filtre dropdown'ı için benzersiz departman listesini hazırlıyoruz
        var departmanlarListesi = await _context.Urunler.Select(u => u.Departman).Distinct().Where(d => d != null).ToListAsync();
        ViewBag.Departmanlar = new SelectList(departmanlarListesi);

        // Diğer kategorisine giren departman isimlerini bulup birleştiriyoruz (Tooltip için)
        var digerDepartmanlarListesi = await _context.Urunler
            .Select(u => u.Departman)
            .Distinct()
            .Where(d => d != "IT" && d != "Üretim" && d != "Depo" && d != null)
            .ToListAsync();

        // "Diğer" kutusunun üzerine gelince açılan tooltip metni: her departman adı + sayısı
        ViewData["DepDigerIsimleri"] = digerDepartmanlar.Any()
    ? string.Join(", ", digerDepartmanlar.Select(d => $"{d.Key} ({d.Value})"))
    : "Ek departman yok";

        // Solteki mavi kart için kategori bazlı ürün çeşidi sayıları
        ViewData["KategoriCesitleri"] = await _context.Kategoriler
            .Select(k => new { KategoriAdi = k.KategoriAdi, Sayi = k.Urunler.Count })
            .ToListAsync();

        // Son hareket bilgisini çekiyoruz
        // (Sayfanın üst kısmındaki "Son İşlem: ..." kutucuğunda gösteriliyor)
        var sonHareket = await _context.StokHareketleri
            .OrderByDescending(h => h.Id)
            .FirstOrDefaultAsync();

        if (sonHareket != null && sonHareket.Tarih != null)
        {
            ViewData["SonIslemBilgisi"] = $"{sonHareket.IslemTuru} ({Convert.ToDateTime(sonHareket.Tarih):dd.MM HH:mm})";
        }
        else
        {
            ViewData["SonIslemBilgisi"] = "Henüz işlem yok";
        }

        // Sayfalama
        // Filtre + sıralama uygulanmış sorgudan (urunlerQuery) sadece istenen sayfa kadar
        // kayıt çekiliyor (Skip/Take) — böylece ürün sayısı artsa bile sayfa performansı düşmüyor.
        int sayfaBoyutu = 10;
        int toplamKayit = await urunlerQuery.CountAsync();
        int toplamSayfa = (int)Math.Ceiling(toplamKayit / (double)sayfaBoyutu);
        if (sayfa < 1) sayfa = 1;
        if (toplamSayfa > 0 && sayfa > toplamSayfa) sayfa = toplamSayfa;

        var sayfalanmisListe = await urunlerQuery
            .Skip((sayfa - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .ToListAsync();

        ViewData["SuankiSayfa"] = sayfa;
        ViewData["ToplamSayfa"] = toplamSayfa;
        ViewData["SearchString"] = searchString;
        ViewData["DepartmanFiltre"] = departmanFiltre;

        return View(sayfalanmisListe);
    }

    // GET: URUNS/Details/5
    // Tek bir ürünün tüm bilgilerini (Kategori ve Tedarikçi bilgisiyle birlikte) gösteren
    // salt-okunur detay sayfası. Hem yetkili hem izleyici kullanıcılar görebilir.
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var urun = await _context.Urunler
            .Include(u => u.Kategori)
            .Include(u => u.Tedarikci)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (urun == null)
        {
            return NotFound();
        }

        return View(urun);
    }

    // GET: URUNS/Create
    // Yeni ürün ekleme formunu gösterir. Kategori ve Tedarikçi dropdown'larını
    // veritabanından doldurur.
    public IActionResult Create()
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index)); // İzleyici ekleyemez!

        ViewData["KategoriId"] = new SelectList(_context.Kategoriler, "Id", "KategoriAdi");
        ViewBag.Tedarikciler = new SelectList(_context.Tedarikciler, "Id", "SirketAdi");
        return View();
    }

    // POST: URUNS/Create
    // Formdan gelen yeni ürünü kaydeder ve aynı zamanda Stok Hareket Geçmişi'ne
    // "Yeni Ürün Eklendi" kaydı düşer. "islemTarihi" alanı doldurulmuşsa (geçmişe
    // dönük bir giriş yapılıyorsa), hareket kaydının tarihi ona göre ayarlanır.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,KategoriId,TedarikciId,UrunAdi,Marka,Model,Ozellikler,StokMiktari,Birim,Departman")] Urun urun, string islemTarihi)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        // 1. SİHİRLİ DOKUNUŞ: Kategori ve Tedarikçi nesnesinin form doğrulamasını bozmasını engelliyoruz.
        ModelState.Remove("Kategori");
        ModelState.Remove("Tedarikci");

        // 2. BURAYA GEÇİCİ OLARAK EKLENEN KONTROL (Hala hata varsa ekrana basacak)
        if (!ModelState.IsValid)
        {
            var hatalar = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            string hataMesaji = string.Join(" | ", hatalar);
            ModelState.AddModelError("", "VALIDATION HATASI: " + hataMesaji);
        }

        // 3. KAYDETME İŞLEMİ
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(urun);
                await _context.SaveChangesAsync();

                string aktifKullanici = HttpContext.Session.GetString("KullaniciAdi") ?? "Bilinmeyen";

                DateTime kaydedilecekTarih = DateTime.Now;

                // Kullanıcı geçmişe dönük bir tarih seçtiyse (örn. "bu ürün aslında geçen ay girildi"),
                // hareket kaydının tarihini o güne, ama şu anki saate göre ayarlıyoruz.
                if (!string.IsNullOrEmpty(islemTarihi))
                {
                    if (DateTime.TryParse(islemTarihi, out DateTime parsedTarih))
                    {
                        kaydedilecekTarih = new DateTime(
                            parsedTarih.Year,
                            parsedTarih.Month,
                            parsedTarih.Day,
                            DateTime.Now.Hour,
                            DateTime.Now.Minute,
                            DateTime.Now.Second
                        );
                    }
                }

                // Yeni ürün ekleme işlemini Stok Hareket Geçmişi'ne de kaydediyoruz (denetim izi / audit log)
                StokHareketi hareket = new StokHareketi
                {
                    UrunAdi = urun.UrunAdi,
                    IslemTuru = "Yeni Ürün Eklendi",
                    Miktar = urun.StokMiktari,
                    Departman = urun.Departman,
                    Kullanici = aktifKullanici,
                    Tarih = kaydedilecekTarih
                };

                _context.Add(hareket);
                await _context.SaveChangesAsync();

                TempData["ToastMesaj"] = "Ürün başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kritik bir hata yakalandı: " + ex.Message + " | İç Hata: " + ex.InnerException?.Message);
            }
        }

        // Doğrulama başarısız olursa, dropdown'ları tekrar doldurup aynı formu geri gösteriyoruz
        ViewData["KategoriId"] = new SelectList(_context.Kategoriler, "Id", "KategoriAdi", urun.KategoriId);
        ViewBag.Tedarikciler = new SelectList(_context.Tedarikciler, "Id", "SirketAdi", urun.TedarikciId);
        return View(urun);
    }

    // GET: URUNS/Edit/5
    // Düzenleme formunu, mevcut ürün bilgileriyle ve doldurulmuş dropdown'larla gösterir.
    public async Task<IActionResult> Edit(int? id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index)); // İzleyici düzenleyemez!

        if (id == null)
        {
            return NotFound();
        }

        var urun = await _context.Urunler.FindAsync(id);
        if (urun == null)
        {
            return NotFound();
        }
        ViewData["KategoriId"] = new SelectList(_context.Kategoriler, "Id", "KategoriAdi", urun.KategoriId);
        ViewBag.Tedarikciler = new SelectList(_context.Tedarikciler, "Id", "SirketAdi", urun.TedarikciId);
        return View(urun);
    }

    // POST: URUNS/Edit/5
    // Ürünü günceller. Ayrıca eski hâliyle (departman/stok) kıyaslayarak
    // Stok Hareket Geçmişi'ne otomatik olarak doğru etiketi düşer:
    //   - Departman değiştiyse           -> "Departman Transferi"
    //   - Sadece stok azaldıysa          -> "Stok Azaltıldı" (islemNedeni ile birlikte, örn. arızalı/zayiat)
    //   - Diğer her türlü değişiklik     -> "Stok Güncellendi"
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,KategoriId,TedarikciId,UrunAdi,Marka,Model,Ozellikler,StokMiktari,Birim,Departman")] Urun urun, string islemNedeni)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        if (id != urun.Id)
        {
            return NotFound();
        }

        // SİHİRLİ DOKUNUŞ: Kategori ve Tedarikçi nesnelerinin form doğrulamasını bozmasını engelliyoruz
        ModelState.Remove("Kategori");
        ModelState.Remove("Tedarikci");

        if (ModelState.IsValid)
        {
            try
            {
                // Güncellemeden ÖNCEKİ hâlini (AsNoTracking ile, takibe almadan) çekiyoruz ki
                // "ne değişti" karşılaştırmasını yapabilelim.
                var eskiUrun = await _context.Urunler.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                string eskiDepartman = eskiUrun?.Departman ?? "Bilinmiyor";
                int eskiStok = eskiUrun?.StokMiktari ?? 0;

                _context.Update(urun);
                await _context.SaveChangesAsync();

                // Ne tür bir değişiklik olduğunu belirleyip hareket geçmişi etiketini seçiyoruz
                string islemTuru = "Stok Güncellendi";
                if (eskiDepartman != urun.Departman)
                {
                    islemTuru = $"Departman Transferi ({eskiDepartman} -> {urun.Departman})";
                }
                else if (urun.StokMiktari < eskiStok)
                {
                    islemTuru = "Stok Azaltıldı";
                }

                string aktifKullanici = HttpContext.Session.GetString("KullaniciAdi") ?? "Bilinmeyen";

                StokHareketi hareket = new StokHareketi
                {
                    UrunAdi = urun.UrunAdi,
                    IslemTuru = islemTuru,
                    Miktar = Math.Abs(urun.StokMiktari - eskiStok),
                    Departman = urun.Departman,
                    Aciklama = islemNedeni, // Stok azaltılıyorsa buraya "arızalı/zayiat" gibi bir sebep girilebilir
                    Kullanici = aktifKullanici,
                    Tarih = DateTime.Now
                };
                _context.Add(hareket);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UrunExists(urun.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["ToastMesaj"] = "Ürün başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["KategoriId"] = new SelectList(_context.Kategoriler, "Id", "KategoriAdi", urun.KategoriId);
        ViewBag.Tedarikciler = new SelectList(_context.Tedarikciler, "Id", "SirketAdi", urun.TedarikciId);
        return View(urun);
    }

    // GET: Urun/Transfer/5
    // Bir ürünü ya başka bir departmana ya da bir müşteriye çıkış olarak
    // gönderme formunu hazırlar (mevcut stok/departman bilgisiyle birlikte).
    public async Task<IActionResult> Transfer(int id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index)); // İzleyici transfer yapamaz!

        var urun = await _context.Urunler.FindAsync(id);
        if (urun == null)
        {
            return NotFound();
        }

        ViewBag.UrunAdi = urun.UrunAdi;
        ViewBag.MevcutDepartman = urun.Departman;
        ViewBag.MevcutStok = urun.StokMiktari;

        ViewBag.Musteriler = new SelectList(_context.Musteriler, "Id", "MusteriAdi");

        return View(urun);
    }

    // POST: Urun/Transfer/5
    // İki farklı senaryoyu tek action içinde yönetir:
    //   Senaryo 1 (musteriId doluysa): Ürün müşteriye satılıyor/gönderiliyor.
    //     Stok sadece düşer, başka bir departmana eklenmez (ürün şirketten çıkıyor).
    //   Senaryo 2 (hedefDepartman doluysa): Ürün başka bir departmana transfer ediliyor.
    //     Kaynaktan düşer, hedef departmanda aynı üründen varsa miktarına eklenir,
    //     yoksa o departman için yeni bir ürün satırı oluşturulur.
    // Her iki senaryoda da işlem Stok Hareket Geçmişi'ne kaydedilir.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(int id, int transferMiktari, string? hedefDepartman, int? musteriId)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        var kaynakUrun = await _context.Urunler.FindAsync(id);
        if (kaynakUrun == null)
        {
            return NotFound();
        }

        if (transferMiktari <= 0 || transferMiktari > kaynakUrun.StokMiktari)
        {
            ModelState.AddModelError("", "Geçersiz transfer miktarı!");
            ViewBag.UrunAdi = kaynakUrun.UrunAdi;
            ViewBag.MevcutDepartman = kaynakUrun.Departman;
            ViewBag.MevcutStok = kaynakUrun.StokMiktari;
            ViewBag.Musteriler = new SelectList(_context.Musteriler, "Id", "MusteriAdi");
            return View(kaynakUrun);
        }

        string aktifKullanici = HttpContext.Session.GetString("KullaniciAdi") ?? "Bilinmeyen";

        //  SENARYO 1: Müşteriye Çıkış (stok düşer, başka bir departmana eklenmez)
        if (musteriId.HasValue)
        {
            kaynakUrun.StokMiktari -= transferMiktari;
            _context.Update(kaynakUrun);

            var musteri = await _context.Musteriler.FindAsync(musteriId.Value);
            string musteriAdi = musteri?.MusteriAdi ?? "Bilinmeyen Müşteri";

            StokHareketi cikisHareketi = new StokHareketi
            {
                UrunAdi = kaynakUrun.UrunAdi,
                IslemTuru = $"Müşteriye Çıkış ({musteriAdi})",
                Miktar = transferMiktari,
                Departman = kaynakUrun.Departman,
                MusteriId = musteriId,
                Kullanici = aktifKullanici,
                Tarih = DateTime.Now
            };
            _context.Add(cikisHareketi);

            await _context.SaveChangesAsync();
            TempData["ToastMesaj"] = "Ürün müşteriye başarıyla teslim edildi.";
            return RedirectToAction(nameof(Index));
        }

        //  SENARYO 2: Departmanlar Arası Transfer
        if (string.IsNullOrEmpty(hedefDepartman))
        {
            ModelState.AddModelError("", "Hedef departman veya müşteri seçilmedi!");
            ViewBag.UrunAdi = kaynakUrun.UrunAdi;
            ViewBag.MevcutDepartman = kaynakUrun.Departman;
            ViewBag.MevcutStok = kaynakUrun.StokMiktari;
            ViewBag.Musteriler = new SelectList(_context.Musteriler, "Id", "MusteriAdi");
            return View(kaynakUrun);
        }

        kaynakUrun.StokMiktari -= transferMiktari;
        _context.Update(kaynakUrun);

        // Hedef departmanda aynı üründen zaten varsa miktarını artırıyoruz,
        // yoksa o departman için sıfırdan yeni bir ürün satırı oluşturuyoruz.
        var hedefUrun = await _context.Urunler
            .FirstOrDefaultAsync(u => u.UrunAdi == kaynakUrun.UrunAdi && u.Departman == hedefDepartman);

        if (hedefUrun != null)
        {
            hedefUrun.StokMiktari += transferMiktari;
            _context.Update(hedefUrun);
        }
        else
        {
            Urun yeniDepartmanUrunu = new Urun
            {
                UrunAdi = kaynakUrun.UrunAdi,
                Marka = kaynakUrun.Marka,
                Model = kaynakUrun.Model,
                Ozellikler = kaynakUrun.Ozellikler,
                KategoriId = kaynakUrun.KategoriId,
                Birim = kaynakUrun.Birim,
                Departman = hedefDepartman,
                StokMiktari = transferMiktari
            };
            _context.Add(yeniDepartmanUrunu);
        }

        StokHareketi hareket = new StokHareketi
        {
            UrunAdi = kaynakUrun.UrunAdi,
            IslemTuru = $"Departman Transferi ({kaynakUrun.Departman} -> {hedefDepartman})",
            Miktar = transferMiktari,
            Departman = hedefDepartman,
            Kullanici = aktifKullanici,
            Tarih = DateTime.Now
        };
        _context.Add(hareket);

        await _context.SaveChangesAsync();
        TempData["ToastMesaj"] = "Ürün departmana başarıyla transfer edildi.";
        return RedirectToAction(nameof(Index));
    }

    // GET: URUNS/Delete/5
    // Silme öncesi onay ekranını, ürünün tüm bilgileriyle birlikte gösterir.
    public async Task<IActionResult> Delete(int? id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index)); // İzleyici siliş sayfasını göremez!

        if (id == null)
        {
            return NotFound();
        }

        var urun = await _context.Urunler
            .Include(u => u.Kategori)
            .Include(u => u.Tedarikci)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (urun == null)
        {
            return NotFound();
        }

        return View(urun);
    }

    // POST: URUNS/Delete/5
    // Ürünü veritabanından siler. Silmeden önce "Ürün Silindi" kaydını
    // Stok Hareket Geçmişi'ne düşer ki silinen ürünün geçmişte var olduğu bilgisi kaybolmasın.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (IsIzleyici()) return RedirectToAction(nameof(Index));

        var urun = await _context.Urunler.FindAsync(id);
        if (urun != null)
        {
            string aktifKullanici = HttpContext.Session.GetString("KullaniciAdi") ?? "Bilinmeyen";

            StokHareketi hareket = new StokHareketi
            {
                UrunAdi = urun.UrunAdi,
                IslemTuru = "Ürün Silindi",
                Miktar = urun.StokMiktari,
                Departman = urun.Departman,
                Kullanici = aktifKullanici,
                Tarih = DateTime.Now
            };
            _context.Add(hareket);

            _context.Urunler.Remove(urun);
            await _context.SaveChangesAsync();

            TempData["ToastMesaj"] = "Ürün silindi.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Verilen id'ye sahip bir ürünün hâlâ var olup olmadığını kontrol eder.
    // (Eşzamanlılık hatası - DbUpdateConcurrencyException - yakalandığında kullanılıyor)
    private bool UrunExists(int id)
    {
        return _context.Urunler.Any(e => e.Id == id);
    }

    // GET: StokHareketleri
    // Tüm stok hareketlerini (ekleme, transfer, satış, silme vb.) listeler.
    // İşlem Türü veya İşlem Tarihi'ne göre sıralanabilir; varsayılan sıralama
    // en yeni işlem en üstte olacak şekildedir.
    public async Task<IActionResult> StokHareketleri(string sortOrder = null)
    {
        ViewData["TarihSiralama"] = sortOrder == "tarih_asc" ? "tarih_desc" : "tarih_asc";
        ViewData["IslemSiralama"] = sortOrder == "islem_asc" ? "islem_desc" : "islem_asc";

        var hareketlerQuery = _context.StokHareketleri.AsQueryable();

        hareketlerQuery = sortOrder switch
        {
            "tarih_asc" => hareketlerQuery.OrderBy(h => h.Tarih),
            "islem_asc" => hareketlerQuery.OrderBy(h => h.IslemTuru),
            "islem_desc" => hareketlerQuery.OrderByDescending(h => h.IslemTuru),
            _ => hareketlerQuery.OrderByDescending(h => h.Tarih),
        };

        var hareketler = await hareketlerQuery.ToListAsync();
        return View(hareketler);
    }

    // GET: Aylik Stok Grafiği Verisi (JSON - Birim Filtreli)
    // Grafik Raporları sayfasındaki Chart.js grafiğini besleyen API uç noktası.
    // Sadece "Yeni Ürün Eklendi" tipindeki hareketleri, isteğe bağlı olarak
    // departman/birime göre filtreleyip aya göre gruplayarak JSON döner.
    [HttpGet]
    public async Task<IActionResult> GetAylikStokVerileri(string? birimFiltre, string? departmanFiltre)
    {
        var hareketlerQuery = _context.StokHareketleri
            .Where(h => h.IslemTuru == "Yeni Ürün Eklendi");

        if (!string.IsNullOrEmpty(departmanFiltre))
        {
            hareketlerQuery = hareketlerQuery.Where(h => h.Departman == departmanFiltre);
        }

        var hareketler = await hareketlerQuery.ToListAsync();

        // StokHareketi tablosunda "Birim" bilgisi tutulmadığı için, ürün adı üzerinden
        // Urunler tablosuyla eşleştirip birim bilgisini buradan alıyoruz.
        var sorgu = from h in hareketler
                    join u in _context.Urunler on h.UrunAdi equals u.UrunAdi into urunGroup
                    from u in urunGroup.DefaultIfEmpty()
                    select new
                    {
                        Hareket = h,
                        Birim = u != null ? u.Birim : "Adet"
                    };

        if (!string.IsNullOrEmpty(birimFiltre))
        {
            sorgu = sorgu.Where(x => x.Birim.ToLower() == birimFiltre.ToLower());
        }

        // Ay/yıla göre grupla ve o aydaki toplam miktarı hesapla
        var veri = sorgu
            .GroupBy(x => new { x.Hareket.Tarih.Year, x.Hareket.Tarih.Month })
            .Select(g => new {
                AyYil = $"{g.Key.Month:D2}.{g.Key.Year}",
                ToplamMiktar = g.Sum(x => x.Hareket.Miktar),
                SiralamaTarih = new DateTime(g.Key.Year, g.Key.Month, 1)
            })
            .OrderBy(x => x.SiralamaTarih)
            .ToList();

        return Json(veri);
    }

    // GET: Raporlar (Aylık Grafik Sayfası)
    // Grafik Raporları sayfasını açar; departman filtre dropdown'ını doldurur.
    // Grafiğin kendisi bu action'dan değil, GetAylikStokVerileri'den (yukarıda) beslenir.
    public async Task<IActionResult> Raporlar()
    {
        ViewBag.KullaniciAdi = HttpContext.Session.GetString("KullaniciAdi") ?? "Misafir";
        ViewBag.UserRole = HttpContext.Session.GetString("UserRole") ?? "Izleyici";

        ViewBag.Departmanlar = await _context.Urunler
            .Select(u => u.Departman)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        return View();
    }

    // GET: ExcelExport
    // Filtrelenmiş ürün listesini CSV formatında dosya olarak indirir.
    // Not: Dosya uzantısı/adı "Excel" gibi görünse de teknik olarak bir CSV (noktalı
    // virgülle ayrılmış metin) dosyasıdır — Excel bu formatı doğrudan açabildiği için
    // kullanıcı deneyimi açısından fark etmez, ama .xlsx değildir.
    [HttpGet]
    public async Task<IActionResult> ExcelExport(string searchString, string departmanFiltre)
    {
        var urunler = _context.Urunler.Include(u => u.Kategori).AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            urunler = urunler.Where(s => s.UrunAdi.Contains(searchString) || s.Marka.Contains(searchString) || s.Model.Contains(searchString));
        }

        if (!string.IsNullOrEmpty(departmanFiltre))
        {
            urunler = urunler.Where(u => u.Departman == departmanFiltre);
        }

        var liste = await urunler.ToListAsync();

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Urun Adi;Marka;Model;Stok Miktari;Birim;Kategori;Departman;Ozellikler");

        foreach (var item in liste)
        {
            builder.AppendLine($"{item.UrunAdi};{item.Marka};{item.Model};{item.StokMiktari};{item.Birim};{item.Kategori?.KategoriAdi};{item.Departman};{item.Ozellikler}");
        }

        // UTF-8 BOM (byte order mark) ekleniyor ki Excel Türkçe karakterleri (ğ, ş, ü vb.)
        // bozuk göstermesin.
        byte[] buffer = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();

        return File(buffer, "text/csv", $"StokRaporu_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }
}