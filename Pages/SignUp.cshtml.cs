using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fin_sCore.Pages;

public class SignUpModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost(string gsm, string firstName, string lastName, string tckno, string email)
    {
        if (string.IsNullOrEmpty(gsm))
        {
            return Page();
        }

        // Validate GSM
        bool isPhone = System.Text.RegularExpressions.Regex.IsMatch(gsm, @"^05\d{9}$");
        if (!isPhone)
        {
            return Page();
        }

        // Store identifier in session
        HttpContext.Session.SetString("Identifier", gsm);
        
        // Reset attempt count
        HttpContext.Session.SetInt32("OtpAttemptCount", 0);

        // Handle signup logic here (save user to database, etc.)
        // For now, redirect to SMS verification
        return RedirectToPage("/SmsVerification", new { identifier = gsm });
    }
}

