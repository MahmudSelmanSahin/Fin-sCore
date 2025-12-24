# Dashboard Geliştirme Kılavuzu

## 🎯 Genel Bakış

Dashboard sayfası, İnteraktif Şube platformunun ana ekranıdır. Kullanıcılar bu ekrandan tüm finansal bilgilerine erişebilir ve hızlı işlemler gerçekleştirebilir.

## 📁 Dosya Yapısı

### Backend (C# Razor Pages)
```
Pages/
├── Dashboard.cshtml          # View (HTML + Razor syntax)
└── Dashboard.cshtml.cs       # PageModel (C# backend logic)
```

### Frontend (SCSS)
```
Styles/
├── pages/
│   └── _dashboard.scss       # Dashboard özel stilleri
└── components/
    └── _notifications.scss   # Bildirim dropdown stilleri
```

### JavaScript
```
wwwroot/
└── js/
    └── dashboard.js          # Dashboard interaktif özellikleri
```

### CMS Verileri (JSON)
```
wwwroot/
└── data/
    └── help_center.json      # Yardım merkezi içerikleri
```

## 🎨 Dashboard Bileşenleri

### 1. Header Section
- **Hoş Geldiniz Mesajı**: Kullanıcı adı ile kişiselleştirilmiş
- **Bildirim Butonu**: Yeni bildirimleri gösterir (badge ile sayı)
- **Profil Butonu**: Profil sayfasına yönlendirir

```html
<header class="dashboard__header">
  <div class="dashboard__welcome">
    <h1 class="dashboard__title">Hoş Geldiniz, @Model.UserName</h1>
  </div>
  <div class="dashboard__user_actions">
    <button class="btn_notification">...</button>
    <a href="/Profile" class="btn_profile">...</a>
  </div>
</header>
```

### 2. Credit Summary Cards (API Servisi)
Üç adet özet kart:

#### a) Toplam Kredi Limiti
- Toplam limit
- Kullanılabilir limit
- Kullanılan limit

#### b) Kredi Skoru
- Skor değeri (0-1800 arası)
- Görsel progress bar
- Durum etiketi (Mükemmel/İyi/Orta)

#### c) Aktif Krediler
- Aktif kredi sayısı
- Hızlı özet bilgi

```csharp
// Backend'den gelen veriler
public decimal TotalCreditLimit { get; set; } = 150000.00m;
public decimal UsedCreditLimit { get; set; } = 45000.00m;
public int CreditScore { get; set; } = 1450;
```

### 3. Quick Actions Grid
Hızlı erişim kartları:
- Kredi Hesaplama
- Kredi Başvurusu
- Ödeme Yap
- Raporlar

Her kart:
- İkon (SVG)
- Başlık
- Açıklama
- Link

### 4. Active Loans Section (API Servisi)
Kullanıcının aktif kredilerini listeler:

**Her kredi kartında:**
- Kredi tipi (İhtiyaç/Konut/Taşıt)
- Kredi tutarı
- Kalan borç
- Aylık taksit
- Sonraki ödeme tarihi
- Progress bar (tamamlanma yüzdesi)
- Detaylar ve Ödeme butonları

```csharp
public class ActiveLoanModel
{
    public string LoanType { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime NextPaymentDate { get; set; }
    public int InstallmentNumber { get; set; }
    public int RemainingInstallments { get; set; }
}
```

### 5. Help Center (CMS - JSON Verisi)
Yardım merkezi kartları JSON'dan yüklenir:

**JSON Yapısı:**
```json
{
  "title": "Kredi Başvurusu Nasıl Yapılır?",
  "description": "Kredi başvuru sürecini adım adım öğrenin",
  "icon": "document",
  "category": "Kredi İşlemleri",
  "link": "/help/credit-application",
  "order": 1,
  "isActive": true
}
```

**Desteklenen İkonlar:**
- `document`: Döküman ikonu
- `calculator`: Hesap makinesi
- `help`: Soru işareti
- `phone`: Telefon

## 🎯 İsimlendirme Kuralları

### CSS Classes (snake_case)
```scss
.dashboard                    // Block
.dashboard__header            // Element
.dashboard__header_content    // Sub-element
.credit_card                  // Block
.credit_card__icon            // Element
.credit_card__icon--blue      // Modifier
```

### JavaScript (snake_case)
```javascript
var $notification_btn = $('#notificationBtn');
function init_animations() { }
function show_notification_dropdown() { }
```

### C# (PascalCase)
```csharp
public class DashboardModel { }
public string UserName { get; set; }
public List<ActiveLoanModel> ActiveLoans { get; set; }
```

## 🎨 Renk Paleti

```scss
$color_white: #FFFFFF;
$color_dark_navy: #222854;
$color_primary_blue: #2E6DF8;
$color_dark_blue: #0056B3;
$color_light_blue: #E6F7FF;
```

## 📱 Responsive Breakpoints

```scss
@media (max-width: 768px) {
  // Tablet
  .dashboard__credit_summary {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 480px) {
  // Mobile
  .quick_actions_grid {
    grid-template-columns: 1fr;
  }
}
```

## ⚡ JavaScript Özellikleri

### 1. Animasyonlar
```javascript
function init_animations() {
  // Kredi skoru animasyonu
  // Progress bar animasyonları
  // Kartların sıralı fade-in
}
```

### 2. Bildirim Sistemi
```javascript
function show_notification_dropdown() {
  // Bildirim dropdown'ını gösterir
  // Backend'den bildirimler çekilir (TODO)
}
```

### 3. İnteraktif Kartlar
```javascript
function init_interactive_cards() {
  // Hover efektleri
  // Analytics tracking
}
```

### 4. Sayfa Görünürlük Takibi
```javascript
document.addEventListener('visibilitychange', function() {
  if (!document.hidden) {
    refresh_dashboard_data();
  }
});
```

## 🔄 API Entegrasyonu (TODO)

### Gerekli Endpoint'ler:

1. **GET /api/user/credit-summary**
   - Kredi limiti
   - Kredi skoru
   - Kullanım bilgileri

2. **GET /api/user/active-loans**
   - Aktif krediler listesi
   - Detaylı kredi bilgileri

3. **GET /api/notifications**
   - Kullanıcı bildirimleri
   - Okunmamış sayısı

4. **GET /api/user/profile**
   - Kullanıcı bilgileri
   - Profil ayarları

## 🚀 Geliştirme İş Akışı

### 1. SCSS Değişikliği
```bash
# Watch mode (otomatik derleme)
npm run sass:watch

# Tek seferlik derleme
npm run sass:build
```

### 2. Backend Değişikliği
```bash
# Build
dotnet build

# Run
dotnet run
```

### 3. JavaScript Değişikliği
- Değişiklikler otomatik yüklenir
- Tarayıcıda hard refresh: `Cmd + Shift + R` (Mac)

## ✅ Test Checklist

- [ ] Dashboard sayfası yükleniyor
- [ ] Kredi özet kartları görünüyor
- [ ] Kredi skoru animasyonu çalışıyor
- [ ] Progress bar'lar animasyonlu
- [ ] Hızlı işlem kartları tıklanabilir
- [ ] Aktif krediler listeleniyor
- [ ] Yardım merkezi kartları JSON'dan yükleniyor
- [ ] Bildirim butonu çalışıyor
- [ ] Responsive tasarım mobilde çalışıyor
- [ ] Hover efektleri aktif

## 🐛 Bilinen Sorunlar ve Çözümler

### Sorun: SCSS derlenmiyor
**Çözüm:**
```bash
npm install
npm run sass:build
```

### Sorun: JSON verisi yüklenmiyor
**Çözüm:**
- `wwwroot/data/help_center.json` dosyasının var olduğundan emin olun
- JSON syntax'ının doğru olduğunu kontrol edin

### Sorun: Animasyonlar çalışmıyor
**Çözüm:**
- jQuery'nin yüklendiğinden emin olun
- `dashboard.js` dosyasının import edildiğini kontrol edin
- Browser console'da hata olup olmadığını kontrol edin

## 📚 Ek Kaynaklar

- [BEM Metodolojisi](http://getbem.com/)
- [SCSS Guide](https://sass-lang.com/guide)
- [.NET Razor Pages](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/)
- [jQuery Documentation](https://api.jquery.com/)

## 🔐 Güvenlik Notları

- ❌ `@Html.Raw()` kullanmayın (XSS riski)
- ✅ Tüm kullanıcı girdilerini validate edin
- ✅ Finansal veriler için `decimal` kullanın
- ✅ API çağrılarında authentication token kullanın

## 📞 Destek

Sorun yaşarsanız:
1. `.cursorrules` dosyasını kontrol edin
2. Linter hatalarını kontrol edin: `read_lints`
3. Terminal loglarını inceleyin

---

**Son Güncelleme:** 20 Aralık 2025
**Versiyon:** 1.0.0




