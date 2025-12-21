using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fin_sCore.Services;

namespace Fin_sCore.Pages;

public class SmsVerificationModel : PageModel
{
    private readonly AuthService _authService;
    private const int MAX_ATTEMPTS = 3;

    public SmsVerificationModel(AuthService authService)
    {
        _authService = authService;
    }

    public string? Gsm { get; set; }
    public string? MaskedGsm { get; set; }
    public int AttemptCount { get; set; }
    public int RemainingAttempts { get; set; }
    public bool ShowRecaptcha { get; set; }
    public bool NewSmsSent { get; set; }

    public IActionResult OnGet()
    {
        // Check if user came from login
        var tckn = HttpContext.Session.GetString("TCKN");
        var gsm = HttpContext.Session.GetString("GSM");
        
        if (string.IsNullOrEmpty(tckn) || string.IsNullOrEmpty(gsm))
        {
            return RedirectToPage("/Index");
        }

        Gsm = gsm;
        MaskedGsm = MaskGsm(gsm);
        AttemptCount = HttpContext.Session.GetInt32("OtpAttemptCount") ?? 0;
        RemainingAttempts = MAX_ATTEMPTS - AttemptCount;
        
        // Show reCAPTCHA if max attempts reached (new SMS was sent)
        ShowRecaptcha = HttpContext.Session.GetString("RequireRecaptcha") == "true";
        NewSmsSent = HttpContext.Session.GetString("NewSmsSent") == "true";
        
        // Clear NewSmsSent flag after reading
        if (NewSmsSent)
        {
            HttpContext.Session.Remove("NewSmsSent");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? otpCode, string? recaptchaToken)
    {
        var tckn = HttpContext.Session.GetString("TCKN");
        var gsm = HttpContext.Session.GetString("GSM");
        
        if (string.IsNullOrEmpty(tckn) || string.IsNullOrEmpty(gsm))
        {
            return RedirectToPage("/Index");
        }

        Gsm = gsm;
        MaskedGsm = MaskGsm(gsm);
        AttemptCount = HttpContext.Session.GetInt32("OtpAttemptCount") ?? 0;
        ShowRecaptcha = HttpContext.Session.GetString("RequireRecaptcha") == "true";
        
        // If reCAPTCHA is required, verify it for EVERY attempt
        if (ShowRecaptcha)
        {
            if (string.IsNullOrEmpty(recaptchaToken))
            {
                TempData["Error"] = "Lütfen robot olmadığınızı doğrulayın";
                RemainingAttempts = MAX_ATTEMPTS - AttemptCount;
                return Page();
            }
            // reCAPTCHA verified for this attempt - but keep requiring it for next attempts
            // Do NOT remove RequireRecaptcha - it stays until successful login
        }

        if (string.IsNullOrEmpty(otpCode) || otpCode.Length != 6)
        {
            TempData["Error"] = "Lütfen 6 haneli doğrulama kodunu giriniz";
            RemainingAttempts = MAX_ATTEMPTS - AttemptCount;
            return Page();
        }

        // Increment attempt count
        AttemptCount++;
        HttpContext.Session.SetInt32("OtpAttemptCount", AttemptCount);
        RemainingAttempts = MAX_ATTEMPTS - AttemptCount;

        // Verify OTP using the real API
        var verifyResult = await _authService.VerifyOtp(otpCode);

        if (verifyResult.Success)
        {
            // Reset everything on successful login
            HttpContext.Session.SetInt32("OtpAttemptCount", 0);
            HttpContext.Session.Remove("RequireRecaptcha");
            
            // Issue token
            var tokenResult = _authService.IssueToken(gsm);
            if (tokenResult.Success)
            {
                HttpContext.Session.SetString("AuthToken", tokenResult.Token!);
                HttpContext.Session.SetString("TokenExpiry", tokenResult.Expiry.ToString("o"));
                
                // Clear OTP data
                HttpContext.Session.Remove("OtpCode");
                HttpContext.Session.Remove("OtpId");
                HttpContext.Session.Remove("SmsVerificationId");
                
                return RedirectToPage("/Dashboard");
            }
        }

        // Check if max attempts reached after this attempt
        if (AttemptCount >= MAX_ATTEMPTS)
        {
            // Do NOT send new OTP automatically - require reCAPTCHA for all future attempts
            HttpContext.Session.SetString("RequireRecaptcha", "true");
            
            TempData["Warning"] = "3 hatalı deneme! Yeni kod almak için 'SMS'i Tekrar Gönder' butonunu kullanın.";
            
            return RedirectToPage("/SmsVerification");
        }

        TempData["Error"] = "Kodu yanlış girdiniz";
        return Page();
    }

    public async Task<IActionResult> OnPostResendOtpAsync()
    {
        var tckn = HttpContext.Session.GetString("TCKN");
        var gsm = HttpContext.Session.GetString("GSM");
        
        if (string.IsNullOrEmpty(tckn) || string.IsNullOrEmpty(gsm))
        {
            return RedirectToPage("/Index");
        }

        // Check if captcha is required (after 3 failed attempts)
        var requireRecaptcha = HttpContext.Session.GetString("RequireRecaptcha") == "true";
        
        // Reset attempt count but keep RequireRecaptcha flag if it was set
        HttpContext.Session.SetInt32("OtpAttemptCount", 0);
        // Note: RequireRecaptcha stays - user must always verify with CAPTCHA after 3 failures
        
        // Generate and send new OTP
        var otpResult = await _authService.SendOtp(tckn, gsm);
        
        if (otpResult.Success)
        {
            // Store new OTP info
            HttpContext.Session.SetString("OtpCode", otpResult.OtpCode ?? "");
            HttpContext.Session.SetInt32("OtpId", otpResult.OtpId);
            HttpContext.Session.SetInt32("SmsVerificationId", otpResult.SmsVerificationId);
            HttpContext.Session.SetString("OtpSentTime", DateTime.Now.ToString("o"));
            
            if (requireRecaptcha)
            {
                TempData["Success"] = "Yeni doğrulama kodu gönderildi. Robot doğrulaması gerekli.";
            }
            else
            {
                TempData["Success"] = "Yeni doğrulama kodu gönderildi";
            }
        }
        else
        {
            TempData["Error"] = otpResult.Message ?? "Kod gönderilemedi";
        }

        return RedirectToPage("/SmsVerification");
    }

    private string MaskGsm(string gsm)
    {
        if (string.IsNullOrEmpty(gsm) || gsm.Length < 7)
            return gsm;
            
        // Format: +90 506 *** ** 07
        var fullGsm = gsm.StartsWith("0") ? gsm : "0" + gsm;
        if (fullGsm.Length == 11)
        {
            // 05064795407 → +90 506 *** ** 07
            var areaCode = fullGsm.Substring(1, 3); // 506
            var lastTwo = fullGsm.Substring(9, 2);  // 07
            return $"+90 {areaCode} *** ** {lastTwo}";
        }
        return gsm;
    }
}
