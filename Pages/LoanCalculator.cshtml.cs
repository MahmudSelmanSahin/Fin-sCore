using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fin_sCore.Pages
{
    public class LoanCalculatorModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Authentication kontrolü
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }
    }
}
