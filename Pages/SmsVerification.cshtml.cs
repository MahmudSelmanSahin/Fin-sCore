using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fin_sCore.Services;

namespace Fin_sCore.Pages;

public class SmsVerificationModel : PageModel
{
    private readonly AuthService _authService;

    public SmsVerificationModel(AuthService authService)
    {
        _authService = authService;
    }

    public string? Identifier { get; set; }
    public bool OtpSent { get; set; }
    public string? OtpCode { get; set; } // For demo only

    public void OnGet(string? identifier)
    {
        Identifier = identifier ?? HttpContext.Session.GetString("Identifier");
        
        if (string.IsNullOrEmpty(Identifier))
        {
            Response.Redirect("/");
            return;
        }

        // Check if OTP was already sent
        OtpSent = HttpContext.Session.GetString($"OtpSent_{Identifier}") == "true";
        
        // If OTP not sent, send it now
        if (!OtpSent)
        {
            var response = _authService.SendOtp(Identifier);
            if (response.Success)
            {
                HttpContext.Session.SetString($"OtpSent_{Identifier}", "true");
                OtpSent = true;
                OtpCode = response.OtpCode; // For demo only - remove in production
            }
        }
    }

    public IActionResult OnPost(string? identifier, string? otpCode)
    {
        Identifier = identifier ?? HttpContext.Session.GetString("Identifier");
        
        if (string.IsNullOrEmpty(Identifier) || string.IsNullOrEmpty(otpCode))
        {
            return Page();
        }

        // Get current attempt count
        int attemptCount = HttpContext.Session.GetInt32("OtpAttemptCount") ?? 0;
        attemptCount++;
        HttpContext.Session.SetInt32("OtpAttemptCount", attemptCount);

        // Verify OTP
        var verifyResponse = _authService.VerifyOtp(Identifier, otpCode);

        if (verifyResponse.Success)
        {
            // Reset attempt count on success
            HttpContext.Session.SetInt32("OtpAttemptCount", 0);
            
            // Issue token (AuthService/IssueToken)
            var tokenResponse = _authService.IssueToken(Identifier);
            if (tokenResponse.Success)
            {
                HttpContext.Session.SetString("AuthToken", tokenResponse.Token);
                HttpContext.Session.SetString("TokenExpiry", tokenResponse.Expiry.ToString());
                
                // Redirect to dashboard
                return RedirectToPage("/Dashboard");
            }
        }

        // Check if max attempts reached
        if (attemptCount >= 3)
        {
            // Redirect to additional security page
            return RedirectToPage("/AdditionalSecurity", new { identifier = Identifier });
        }

        // Return to page with error
        TempData["Error"] = verifyResponse.Message;
        return RedirectToPage("/SmsVerification", new { identifier = Identifier });
    }

    public IActionResult OnPostResendOtp(string? identifier)
    {
        Identifier = identifier ?? HttpContext.Session.GetString("Identifier");
        
        if (string.IsNullOrEmpty(Identifier))
        {
            return RedirectToPage("/");
        }

        // Reset attempt count
        HttpContext.Session.SetInt32("OtpAttemptCount", 0);
        
        // Send new OTP
        var response = _authService.SendOtp(Identifier);
        if (response.Success)
        {
            HttpContext.Session.SetString($"OtpSent_{Identifier}", "true");
            TempData["Success"] = "Yeni kod gönderildi";
        }
        else
        {
            TempData["Error"] = response.Message;
        }

        return RedirectToPage("/SmsVerification", new { identifier = Identifier });
    }
}

