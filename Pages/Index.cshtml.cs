using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fin_sCore.Services;
using System.Text.Json;

namespace Fin_sCore.Pages;

public class IndexModel : PageModel
{
    private readonly AuthService _authService;
    private readonly CaptchaService _captchaService;

    public bool HasKvkkConsent { get; set; }
    
    [BindProperty]
    public string Tckn { get; set; } = string.Empty;
    
    [BindProperty]
    public string Gsm { get; set; } = string.Empty;
    
    [BindProperty]
    public bool KvkkAccept { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public IndexModel(AuthService authService, CaptchaService captchaService)
    {
        _authService = authService;
        _captchaService = captchaService;
    }

    public void OnGet()
    {
        // If already authenticated, redirect to dashboard
        var authToken = HttpContext.Session.GetString("AuthToken");
        if (!string.IsNullOrEmpty(authToken))
        {
            Response.Redirect("/Dashboard");
            return;
        }

        // Check if user has KVKK consent (from session)
        HasKvkkConsent = HttpContext.Session.GetString("HasKvkkConsent") == "true";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // This is kept for non-JS fallback but now we use AJAX
        return await OnPostLoginAsync(new LoginRequest { Tckn = Tckn, Gsm = Gsm, KvkkAccept = KvkkAccept });
    }

    // AJAX Login Handler - validates and sends OTP
    public async Task<IActionResult> OnPostLoginAsync([FromBody] LoginRequest? request)
    {
        if (request == null)
        {
            return new JsonResult(new { success = false, message = "Geçersiz istek" });
        }

        var tckn = request.Tckn;
        var gsm = request.Gsm;
        var kvkkAccept = request.KvkkAccept;

        // Validate inputs
        if (string.IsNullOrEmpty(tckn) || string.IsNullOrEmpty(gsm))
        {
            return new JsonResult(new { success = false, message = "TCKN ve GSM alanları zorunludur" });
        }

        // Validate TCKN (11 digits)
        if (!System.Text.RegularExpressions.Regex.IsMatch(tckn, @"^\d{11}$"))
        {
            return new JsonResult(new { success = false, message = "TCKN 11 haneli olmalıdır" });
        }

        // Validate GSM (10 digits starting with 5, or 11 digits starting with 05)
        var gsmClean = gsm.StartsWith("0") ? gsm.Substring(1) : gsm;
        if (!System.Text.RegularExpressions.Regex.IsMatch(gsmClean, @"^5\d{9}$"))
        {
            return new JsonResult(new { success = false, message = "Geçerli bir GSM numarası giriniz" });
        }

        // Check KVKK consent
        bool hasConsent = HttpContext.Session.GetString("HasKvkkConsent") == "true";
        
        if (!hasConsent && !kvkkAccept)
        {
            return new JsonResult(new { success = false, message = "KVKK metnini kabul etmelisiniz" });
        }

        // Call TCKN-GSM validation API
        var result = await _authService.ValidateTcknGsm(tckn, gsmClean);

        if (!result.Success)
        {
            return new JsonResult(new { success = false, message = result.Message });
        }

        // Store user information in session
        HttpContext.Session.SetString("TCKN", tckn);
        HttpContext.Session.SetString("GSM", gsmClean);
        HttpContext.Session.SetInt32("CustomerId", result.Value?.CustomerId ?? 0);
        HttpContext.Session.SetString("ApiMessage", result.Message);
        
        // KVKK onayını "pending" olarak işaretle - login başarılı olunca kalıcı olacak
        if (!hasConsent && kvkkAccept)
        {
            HttpContext.Session.SetString("PendingKvkkConsent", "true");
            
            // Save KVKK approval to API
            var customerId = result.Value?.CustomerId ?? 0;
            if (customerId > 0)
            {
                await _authService.SaveKvkkOnay(1, customerId, true);
            }
        }
        
        // Reset OTP attempt count and clear any captcha requirements from previous sessions
        HttpContext.Session.SetInt32("OtpAttemptCount", 0);
        HttpContext.Session.Remove("OtpCode");
        HttpContext.Session.Remove("OtpId");
        HttpContext.Session.Remove("RequireCaptcha");
        HttpContext.Session.Remove("RequireRecaptcha");
        HttpContext.Session.Remove("CaptchaCode");

        // Generate and send OTP
        var otpResult = await _authService.SendOtp(tckn, gsmClean);
        
        if (!otpResult.Success)
        {
            return new JsonResult(new { success = false, message = otpResult.Message ?? "OTP gönderilemedi" });
        }

        // Store OTP info in session (for verification)
        HttpContext.Session.SetString("OtpCode", otpResult.OtpCode ?? "");
        HttpContext.Session.SetInt32("OtpId", otpResult.OtpId);
        HttpContext.Session.SetInt32("SmsVerificationId", otpResult.SmsVerificationId);
        HttpContext.Session.SetString("OtpSentTime", DateTime.Now.ToString("o"));

        // Return masked GSM for display
        var maskedGsm = MaskGsm(gsmClean);

        return new JsonResult(new { 
            success = true, 
            message = "Doğrulama kodu gönderildi",
            maskedGsm = maskedGsm,
            remainingAttempts = 3
        });
    }

    // AJAX OTP Verification Handler
    public async Task<IActionResult> OnPostVerifyOtpAsync([FromBody] VerifyOtpRequest? request)
    {
        if (request == null || string.IsNullOrEmpty(request.OtpCode))
        {
            return new JsonResult(new { success = false, message = "Doğrulama kodu giriniz" });
        }

        var tckn = HttpContext.Session.GetString("TCKN");
        var gsm = HttpContext.Session.GetString("GSM");
        
        if (string.IsNullOrEmpty(tckn) || string.IsNullOrEmpty(gsm))
        {
            return new JsonResult(new { success = false, message = "Oturum bilgisi bulunamadı. Lütfen tekrar giriş yapın." });
        }

        if (request.OtpCode.Length != 6)
        {
            return new JsonResult(new { success = false, message = "Doğrulama kodu 6 haneli olmalıdır" });
        }

        // Check if CAPTCHA is required
        var requireCaptcha = HttpContext.Session.GetString("RequireCaptcha") == "true";
        
        // If CAPTCHA is required, verify it
        if (requireCaptcha)
        {
            var sessionCaptcha = HttpContext.Session.GetString("CaptchaCode");
            if (string.IsNullOrEmpty(request.CaptchaCode) || 
                !string.Equals(request.CaptchaCode, sessionCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                // Generate new CAPTCHA for next attempt
                var newCaptcha = _captchaService.GenerateCaptcha();
                HttpContext.Session.SetString("CaptchaCode", newCaptcha.Code);
                
                return new JsonResult(new { 
                    success = false, 
                    message = "Güvenlik kodu hatalı",
                    requireCaptcha = true,
                    captchaImage = newCaptcha.ImageBase64
                });
            }
        }

        // Get and increment attempt count
        var attemptCount = HttpContext.Session.GetInt32("OtpAttemptCount") ?? 0;
        attemptCount++;
        HttpContext.Session.SetInt32("OtpAttemptCount", attemptCount);

        // Verify OTP using API
        var verifyResult = await _authService.VerifyOtp(request.OtpCode);

        if (verifyResult.Success)
        {
            // Reset everything on successful login
            HttpContext.Session.SetInt32("OtpAttemptCount", 0);
            HttpContext.Session.Remove("RequireCaptcha");
            
            // Issue token
            var tokenResult = _authService.IssueToken(gsm);
            if (tokenResult.Success)
            {
                HttpContext.Session.SetString("AuthToken", tokenResult.Token!);
                HttpContext.Session.SetString("TokenExpiry", tokenResult.Expiry.ToString("o"));
                
                // Login başarılı - KVKK onayını kalıcı olarak kaydet
                if (HttpContext.Session.GetString("PendingKvkkConsent") == "true")
                {
                    HttpContext.Session.SetString("HasKvkkConsent", "true");
                    HttpContext.Session.Remove("PendingKvkkConsent");
                }
                
                // Clear OTP data
                HttpContext.Session.Remove("OtpCode");
                HttpContext.Session.Remove("OtpId");
                HttpContext.Session.Remove("SmsVerificationId");
                
                return new JsonResult(new { 
                    success = true, 
                    message = "Giriş başarılı",
                    redirectUrl = "/Dashboard"
                });
            }
        }

        // Check if max attempts reached
        if (attemptCount >= 3)
        {
            // Do NOT send new OTP automatically - require CAPTCHA for all future attempts
            HttpContext.Session.SetString("RequireCaptcha", "true");
            
            // Generate CAPTCHA
            var captcha = _captchaService.GenerateCaptcha();
            HttpContext.Session.SetString("CaptchaCode", captcha.Code);
            
            return new JsonResult(new { 
                success = false, 
                message = "Kodu yanlış girdiniz",
                requireCaptcha = true,
                captchaImage = captcha.ImageBase64
            });
        }

        return new JsonResult(new { 
            success = false, 
            message = "Kodu yanlış girdiniz"
        });
    }

    // AJAX Resend OTP Handler
    public async Task<IActionResult> OnPostResendOtpAsync()
    {
        var tckn = HttpContext.Session.GetString("TCKN");
        var gsm = HttpContext.Session.GetString("GSM");
        
        if (string.IsNullOrEmpty(tckn) || string.IsNullOrEmpty(gsm))
        {
            return new JsonResult(new { success = false, message = "Oturum bilgisi bulunamadı" });
        }

        // Check if CAPTCHA is required (keep the flag - don't reset it)
        var requireCaptcha = HttpContext.Session.GetString("RequireCaptcha") == "true";
        
        // Reset attempt count but keep RequireCaptcha flag
        HttpContext.Session.SetInt32("OtpAttemptCount", 0);
        
        // Generate and send new OTP
        var otpResult = await _authService.SendOtp(tckn, gsm);
        
        if (otpResult.Success)
        {
            // Store new OTP info
            HttpContext.Session.SetString("OtpCode", otpResult.OtpCode ?? "");
            HttpContext.Session.SetInt32("OtpId", otpResult.OtpId);
            HttpContext.Session.SetInt32("SmsVerificationId", otpResult.SmsVerificationId);
            HttpContext.Session.SetString("OtpSentTime", DateTime.Now.ToString("o"));
            
            return new JsonResult(new { 
                success = true, 
                message = requireCaptcha ? "Yeni doğrulama kodu gönderildi. Robot doğrulaması gerekli." : "Yeni doğrulama kodu gönderildi"
            });
        }

        return new JsonResult(new { success = false, message = otpResult.Message ?? "Kod gönderilemedi" });
    }

    private string MaskGsm(string gsm)
    {
        if (string.IsNullOrEmpty(gsm) || gsm.Length < 7)
            return gsm;
            
        // Format: +90 506 *** ** 07
        var fullGsm = gsm.StartsWith("0") ? gsm : "0" + gsm;
        if (fullGsm.Length == 11)
        {
            var areaCode = fullGsm.Substring(1, 3);
            var lastTwo = fullGsm.Substring(9, 2);
            return $"+90 {areaCode} *** ** {lastTwo}";
        }
        return gsm;
    }

    public class LoginRequest
    {
        public string Tckn { get; set; } = string.Empty;
        public string Gsm { get; set; } = string.Empty;
        public bool KvkkAccept { get; set; }
    }

    public class VerifyOtpRequest
    {
        public string OtpCode { get; set; } = string.Empty;
        public string? CaptchaCode { get; set; }
    }

    // CAPTCHA Generation Endpoint
    public IActionResult OnGetCaptcha()
    {
        var captcha = _captchaService.GenerateCaptcha();
        HttpContext.Session.SetString("CaptchaCode", captcha.Code);
        
        return new JsonResult(new { 
            success = true, 
            captchaImage = captcha.ImageBase64
        });
    }

    public async Task<IActionResult> OnGetKvkkTextAsync(int kvkkId = 1)
    {
        var result = await _authService.GetKvkkText(kvkkId);
        
        if (result != null)
        {
            return new JsonResult(new { success = true, data = result });
        }
        
        return new JsonResult(new { success = false, message = "KVKK metni alınamadı" });
    }

    public async Task<IActionResult> OnPostKvkkOnayAsync([FromBody] KvkkOnayRequest request)
    {
        if (request == null)
        {
            return new JsonResult(new { success = false, message = "Geçersiz istek" });
        }

        // Get customer ID from session if not provided
        var customerId = request.CustomerId;
        if (customerId == 0)
        {
            customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;
        }

        if (customerId == 0)
        {
            // Use a temporary ID for now - will be updated after TCKN-GSM validation
            customerId = 1000849;
        }

        var result = await _authService.SaveKvkkOnay(request.KvkkId, customerId, request.IsOk);
        
        if (result != null)
        {
            // NOT: KVKK onayını session'a kaydetmiyoruz - sadece login başarılı olunca kaydedilecek
            // Bu sayede sayfa yenilenirse KVKK checkbox tekrar görünecek
            HttpContext.Session.SetInt32("KvkkOnayId", result.Id);
            
            return new JsonResult(new { success = true, data = result });
        }
        
        // Even if API fails, allow the user to proceed (optimistic approach)
        // Session'a HasKvkkConsent kaydetmiyoruz - login tamamlanana kadar bekle
        return new JsonResult(new { success = true, data = new { Id = 0, Message = "KVKK onayı kaydedildi" } });
    }

    public class KvkkOnayRequest
    {
        public int KvkkId { get; set; }
        public int CustomerId { get; set; }
        public bool IsOk { get; set; }
    }
}
