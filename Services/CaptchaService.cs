using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Fin_sCore.Services;

public class CaptchaService
{
    private readonly Random _random = new Random();
    private const string CHARS = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Karışabilecek karakterler çıkarıldı (I, O, 0, 1)
    
    public CaptchaResult GenerateCaptcha(int length = 5)
    {
        var code = GenerateRandomCode(length);
        var imageBytes = GenerateCaptchaImage(code);
        
        return new CaptchaResult
        {
            Code = code,
            ImageBase64 = Convert.ToBase64String(imageBytes)
        };
    }
    
    private string GenerateRandomCode(int length)
    {
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = CHARS[_random.Next(CHARS.Length)];
        }
        return new string(chars);
    }
    
    private byte[] GenerateCaptchaImage(string code)
    {
        int width = 200;
        int height = 70;
        
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        
        // Yüksek kalite ayarları
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        
        // Beyaz arka plan
        graphics.Clear(Color.White);
        
        // Arka plan gürültüsü - noktalar
        DrawBackgroundNoise(graphics, width, height);
        
        // Gürültü çizgileri
        DrawNoiseLines(graphics, width, height);
        
        // Karakterleri tek tek çiz (eğri ve distorted)
        DrawDistortedText(graphics, code, width, height);
        
        // Üst çizgiler (karakterlerin üzerinden geçen)
        DrawOverlayLines(graphics, width, height);
        
        // Hafif bulanıklık efekti
        ApplyBlurEffect(bitmap);
        
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
    
    private void DrawBackgroundNoise(Graphics graphics, int width, int height)
    {
        // Arka plan noktaları
        for (int i = 0; i < 100; i++)
        {
            int x = _random.Next(width);
            int y = _random.Next(height);
            int size = _random.Next(1, 3);
            
            var grayShade = _random.Next(180, 220);
            using var brush = new SolidBrush(Color.FromArgb(grayShade, grayShade, grayShade));
            graphics.FillEllipse(brush, x, y, size, size);
        }
    }
    
    private void DrawNoiseLines(Graphics graphics, int width, int height)
    {
        // Arka plan çizgileri (açık renkli)
        for (int i = 0; i < 5; i++)
        {
            var grayShade = _random.Next(200, 230);
            using var pen = new Pen(Color.FromArgb(grayShade, grayShade, grayShade), 1);
            
            int x1 = _random.Next(width);
            int y1 = _random.Next(height);
            int x2 = _random.Next(width);
            int y2 = _random.Next(height);
            
            graphics.DrawLine(pen, x1, y1, x2, y2);
        }
    }
    
    private void DrawDistortedText(Graphics graphics, string code, int width, int height)
    {
        var fonts = new[] { "Arial", "Verdana", "Tahoma", "Georgia", "Times New Roman" };
        int charWidth = (width - 20) / code.Length;
        
        for (int i = 0; i < code.Length; i++)
        {
            // Rastgele font ve boyut
            var fontFamily = fonts[_random.Next(fonts.Length)];
            var fontSize = _random.Next(22, 30);
            var fontStyle = _random.Next(2) == 0 ? FontStyle.Bold : FontStyle.Italic | FontStyle.Bold;
            
            using var font = new Font(fontFamily, fontSize, fontStyle);
            
            // Siyah veya koyu mavi renk
            Color textColor;
            if (_random.Next(2) == 0)
            {
                textColor = Color.FromArgb(10, 10, 10); // Siyah
            }
            else
            {
                textColor = Color.FromArgb(0, 30, 100); // Koyu mavi
            }
            
            using var brush = new SolidBrush(textColor);
            
            // Karakter pozisyonu
            float x = 10 + i * charWidth + _random.Next(-3, 4);
            float y = _random.Next(5, 15);
            
            // Dönüş açısı
            float angle = _random.Next(-25, 26);
            
            // Transform uygula
            graphics.TranslateTransform(x + charWidth / 2, height / 2);
            graphics.RotateTransform(angle);
            
            // Karakteri çiz
            var charStr = code[i].ToString();
            var charSize = graphics.MeasureString(charStr, font);
            graphics.DrawString(charStr, font, brush, -charSize.Width / 2, -charSize.Height / 2);
            
            // Transform'u sıfırla
            graphics.ResetTransform();
        }
    }
    
    private void DrawOverlayLines(Graphics graphics, int width, int height)
    {
        // Karakterlerin üzerinden geçen çizgiler
        for (int i = 0; i < 3; i++)
        {
            Color lineColor;
            if (_random.Next(2) == 0)
            {
                lineColor = Color.FromArgb(150, 30, 30, 30); // Yarı saydam siyah
            }
            else
            {
                lineColor = Color.FromArgb(150, 0, 40, 120); // Yarı saydam koyu mavi
            }
            
            using var pen = new Pen(lineColor, _random.Next(1, 3));
            
            // Eğri çizgi (Bezier)
            Point start = new Point(0, _random.Next(height));
            Point end = new Point(width, _random.Next(height));
            Point control1 = new Point(_random.Next(width / 3), _random.Next(height));
            Point control2 = new Point(_random.Next(width / 3, 2 * width / 3), _random.Next(height));
            
            graphics.DrawBezier(pen, start, control1, control2, end);
        }
    }
    
    private void ApplyBlurEffect(Bitmap bitmap)
    {
        // Basit bir bulanıklık efekti (3x3 box blur)
        // Sadece arka plan için hafif blur
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        
        int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
        byte[] rgbValues = new byte[bytes];
        byte[] result = new byte[bytes];
        
        System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);
        
        int stride = bmpData.Stride;
        int width = bitmap.Width;
        int height = bitmap.Height;
        
        // Sadece hafif bir blur (her 3. pixel)
        for (int y = 1; y < height - 1; y += 2)
        {
            for (int x = 1; x < width - 1; x += 2)
            {
                for (int c = 0; c < 3; c++) // R, G, B channels
                {
                    int idx = y * stride + x * 4 + c;
                    int sum = rgbValues[idx] * 4;
                    sum += rgbValues[(y - 1) * stride + x * 4 + c];
                    sum += rgbValues[(y + 1) * stride + x * 4 + c];
                    sum += rgbValues[y * stride + (x - 1) * 4 + c];
                    sum += rgbValues[y * stride + (x + 1) * 4 + c];
                    rgbValues[idx] = (byte)(sum / 8);
                }
            }
        }
        
        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmpData.Scan0, bytes);
        bitmap.UnlockBits(bmpData);
    }
}

public class CaptchaResult
{
    public string Code { get; set; } = string.Empty;
    public string ImageBase64 { get; set; } = string.Empty;
}

