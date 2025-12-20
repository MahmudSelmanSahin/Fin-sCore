# İnteraktif Şube - Web Uygulaması

Modern, etkileşimli bir şube giriş sistemi. .NET Core 8 Razor Pages ile geliştirilmiştir.

## Özellikler

- **Giriş Ekranı**: GSM numarası veya TC Kimlik No ile giriş
- **KVKK Uyumluluğu**: Aydınlatma metni onayı
- **SMS Doğrulama**: 6 haneli kod doğrulama (maksimum 3 deneme)
- **Üye Ol**: Yeni kullanıcı kaydı
- **Modern Tasarım**: Web 2.0 ruhuna uygun, mikro-etkileşimler içeren arayüz

## Teknolojiler

- .NET Core 8
- Razor Pages
- SCSS (Özel Tasarım)
- jQuery
- Modern CSS Animations

## Kurulum

1. .NET 8 SDK'nın yüklü olduğundan emin olun
2. Projeyi çalıştırın:
   ```bash
   dotnet run
   ```
3. Tarayıcınızda `https://localhost:5001` adresine gidin

## SCSS Derleme

SCSS dosyalarını derlemek için:
```bash
sass wwwroot/scss/site.scss wwwroot/css/site.css
```

Veya watch modu için:
```bash
sass --watch wwwroot/scss/site.scss:wwwroot/css/site.css
```

## Sayfalar

- `/` - Giriş sayfası
- `/SmsVerification` - SMS doğrulama sayfası
- `/SignUp` - Üye ol sayfası
- `/Dashboard` - Başarılı giriş sonrası dashboard

## Tasarım Özellikleri

- Gradient arka planlar
- Smooth animasyonlar
- Micro-interactions
- Responsive tasarım
- Modern kart tabanlı arayüz
- Etkileşimli form elemanları

