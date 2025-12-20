using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fin_sCore.Pages;

public class IndexModel : PageModel
{
    public bool HasKvkkConsent { get; set; }

    public void OnGet()
    {
        // If already authenticated, redirect to dashboard
        var authToken = HttpContext.Session.GetString("AuthToken");
        if (!string.IsNullOrEmpty(authToken))
        {
            Response.Redirect("/Dashboard");
            return;
        }

        // Check if user has KVKK consent (simulated - in real app, check from database)
        HasKvkkConsent = HttpContext.Session.GetString("HasKvkkConsent") == "true";
    }

    public IActionResult OnPost(string identifier, bool? kvkkAccept)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return Page();
        }

        // Validate identifier
        bool isPhone = System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^05\d{9}$");
        bool isTCKNO = System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^\d{11}$");

        if (!isPhone && !isTCKNO)
        {
            return Page();
        }

        // Check KVKK consent
        bool hasConsent = HttpContext.Session.GetString("HasKvkkConsent") == "true";
        
        if (!hasConsent)
        {
            // If no consent, require acceptance
            if (kvkkAccept != true)
            {
                return Page();
            }
            
            // Save KVKK consent
            HttpContext.Session.SetString("HasKvkkConsent", "true");
        }

        // Store identifier in session
        HttpContext.Session.SetString("Identifier", identifier);
        
        // Reset attempt count
        HttpContext.Session.SetInt32("OtpAttemptCount", 0);

        // Geliştirme aşamasında direkt Dashboard'a yönlendir
        // Canlıda SMS doğrulama aktif olacak
        // return RedirectToPage("/SmsVerification", new { identifier = identifier });
        return RedirectToPage("/Dashboard");
    }
}

