using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fin_sCore.Pages
{
    public class ReportDetailsModel : PageModel
    {
        private readonly ILogger<ReportDetailsModel> _logger;

        public ReportDetailsModel(ILogger<ReportDetailsModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Authentication kontrolü
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToPage("/Index");
            }

            _logger.LogInformation("ReportDetails page accessed at {Time}", DateTime.UtcNow);
            return Page();
        }
    }
}

