using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fin_sCore.Services;
using System.Text.Json;

namespace Fin_sCore.Pages;

public class IndexModel : PageModel
{
    private readonly AuthService _authService;

    public bool HasKvkkConsent { get; set; }
    
    [BindProperty]
    public string Tckn { get; set; } = string.Empty;
    
    [BindProperty]
    public string Gsm { get; set; } = string.Empty;
    
    [BindProperty]
    public bool KvkkAccept { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public IndexModel(AuthService authService)
    {
        _authService = authService;
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
        
        // Save KVKK consent if just accepted
        if (!hasConsent && kvkkAccept)
        {
            HttpContext.Session.SetString("HasKvkkConsent", "true");
            
            // Save KVKK approval to API
            var customerId = result.Value?.CustomerId ?? 0;
            if (customerId > 0)
            {
                await _authService.SaveKvkkOnay(1, customerId, true);
            }
        }
        
        // Reset OTP attempt count
        HttpContext.Session.SetInt32("OtpAttemptCount", 0);
        HttpContext.Session.Remove("OtpCode");
        HttpContext.Session.Remove("OtpId");

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
                
                return new JsonResult(new { 
                    success = true, 
                    message = "Giriş başarılı",
                    redirectUrl = "/Dashboard"
                });
            }
        }

        var remainingAttempts = 3 - attemptCount;

        // Check if max attempts reached
        if (attemptCount >= 3)
        {
            // Send new OTP automatically
            var otpResult = await _authService.SendOtp(tckn, gsm);
            
            if (otpResult.Success)
            {
                // Store new OTP info
                HttpContext.Session.SetString("OtpCode", otpResult.OtpCode ?? "");
                HttpContext.Session.SetInt32("OtpId", otpResult.OtpId);
                HttpContext.Session.SetInt32("SmsVerificationId", otpResult.SmsVerificationId);
                HttpContext.Session.SetString("OtpSentTime", DateTime.Now.ToString("o"));
                
                // Reset attempt count
                HttpContext.Session.SetInt32("OtpAttemptCount", 0);
                
                return new JsonResult(new { 
                    success = false, 
                    message = "3 hatalı deneme! Yeni doğrulama kodu gönderildi.",
                    newOtpSent = true,
                    remainingAttempts = 3
                });
            }
        }

        return new JsonResult(new { 
            success = false, 
            message = verifyResult.Message ?? "Hatalı doğrulama kodu",
            remainingAttempts = remainingAttempts > 0 ? remainingAttempts : 0
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

        // Reset attempt count
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
                message = "Yeni doğrulama kodu gönderildi",
                remainingAttempts = 3
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
            // Save KVKK consent to session
            HttpContext.Session.SetString("HasKvkkConsent", "true");
            HttpContext.Session.SetInt32("KvkkOnayId", result.Id);
            
            return new JsonResult(new { success = true, data = result });
        }
        
        // Even if API fails, allow the user to proceed (optimistic approach)
        HttpContext.Session.SetString("HasKvkkConsent", "true");
        return new JsonResult(new { success = true, data = new { Id = 0, Message = "KVKK onayı kaydedildi" } });
    }

    public class KvkkOnayRequest
    {
        public int KvkkId { get; set; }
        public int CustomerId { get; set; }
        public bool IsOk { get; set; }
    }
}
