# Fin-sCore - İnteraktif Şube Platformu

## 📋 Proje Hakkında

Fin-sCore, modern ve kullanıcı dostu bir dijital kredi platformudur. .NET 8 Razor Pages teknolojisi ile geliştirilmiştir.

## 🚀 Teknoloji Stack

- **Backend:** .NET 8 (C# Razor Pages)
- **Frontend:** HTML5, Custom SCSS (SASS), jQuery
- **Database:** Entity Framework Core (SQL)
- **Stil Mimarisi:** BEM Metodolojisi (snake_case uyumlu)

## 📁 Proje Yapısı

```
Fin-sCore/
├── Pages/                    # Razor Pages
│   ├── Dashboard.cshtml      # Ana dashboard sayfası
│   ├── Index.cshtml          # Login sayfası
│   └── Shared/               # Paylaşılan layout'lar
├── Styles/                   # SCSS dosyaları
│   ├── abstracts/            # Değişkenler ve mixinler
│   ├── base/                 # Reset ve tipografi
│   ├── components/           # Yeniden kullanılabilir bileşenler
│   └── pages/                # Sayfaya özel stiller
├── wwwroot/                  # Statik dosyalar
│   ├── css/                  # Derlenmiş CSS
│   ├── js/                   # JavaScript dosyaları
│   ├── data/                 # CMS JSON verileri
│   └── img/                  # Görseller
└── Program.cs                # Uygulama giriş noktası
```

## 🎨 Dashboard Özellikleri

### Ana Bileşenler

1. **Kullanıcı Anasayfası (Dashboard)**
   - Kredi limiti özeti
   - Kredi skoru gösterimi
   - Aktif krediler listesi
   - Hızlı işlem kartları

2. **Rapor Detayları (ReportService - API)**
   - Kredi geçmişi raporları
   - Ödeme takvimleri
   - Finansal analizler

3. **Yardım & Destek Merkezi (CMS - JSON)**
   - SSS (Sıkça Sorulan Sorular)
   - Kredi hesaplama araçları
   - İletişim bilgileri
   - KVKK aydınlatma metni

4. **Profil & Hesap Ayarları (UserService - API)**
   - Kullanıcı bilgileri
   - Güvenlik ayarları
   - Bildirim tercihleri

## 🛠️ Kurulum

### Gereksinimler

- .NET 8 SDK
- Node.js (SASS derlemesi için)
- SQL Server (veya LocalDB)

### Adımlar

1. **Projeyi klonlayın:**
```bash
git clone [repository-url]
cd Fin-sCore
```

2. **NuGet paketlerini yükleyin:**
```bash
dotnet restore
```

3. **NPM paketlerini yükleyin:**
```bash
npm install
```

4. **SCSS'yi derleyin:**
```bash
npm run sass:build
```

5. **Veritabanını oluşturun:**
```bash
dotnet ef database update
```

6. **Uygulamayı çalıştırın:**
```bash
dotnet run
```

Uygulama `https://localhost:5001` adresinde çalışacaktır.

## 📝 Geliştirme Kuralları

Proje `.cursorrules` dosyasında tanımlı kurallara göre geliştirilmektedir:

### İsimlendirme Kuralları

| Asset Type | Format | Örnek |
|------------|--------|-------|
| CSS Class/ID | `snake_case` | `.credit_card__header` |
| JavaScript | `snake_case` | `calculate_payment()` |
| Razor Files | `PascalCase` | `Dashboard.cshtml` |
| C# Properties | `PascalCase` | `public decimal LoanAmount` |

### Kritik Kısıtlamalar

- ❌ CSS framework'leri yasak (Tailwind, Bootstrap)
- ❌ Inline style kullanımı yasak
- ❌ `!important` kullanımı yasak
- ❌ `@Html.Raw()` kullanımı yasak (XSS koruması)
- ✅ Finansal hesaplamalar için `decimal` kullanılmalı

## 🎯 SCSS Mimarisi

BEM (Block, Element, Modifier) metodolojisi kullanılmaktadır:

```scss
// Block
.loan_card { }

// Element (Double underscore)
.loan_card__header { }

// Modifier (Double dash)
.loan_card--approved { }
```

### SCSS Derleme

**Development (watch mode):**
```bash
npm run sass:watch
```

**Production:**
```bash
npm run sass:build
```

## 📊 CMS Veri Yapısı

Yardım merkezi verileri JSON formatında saklanır:

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

## 🔐 Güvenlik

- XSS koruması (Raw HTML yasak)
- CSRF token'ları
- KVKK uyumlu veri işleme
- Güvenli form validasyonu

## 📱 Responsive Tasarım

- Mobile-first yaklaşım
- Breakpoint'ler: 480px, 768px, 1024px, 1400px
- Touch-friendly UI elementleri

## 🤝 Katkıda Bulunma

Lütfen katkıda bulunmadan önce `.cursorrules` dosyasını okuyun ve kurallara uygun kod yazın.

## 📄 Lisans

[Lisans bilgisi eklenecek]

## 📞 İletişim

[İletişim bilgileri eklenecek]
