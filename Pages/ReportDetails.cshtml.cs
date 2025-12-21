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

        public void OnGet()
        {
            _logger.LogInformation("ReportDetails page accessed at {Time}", DateTime.UtcNow);
        }
    }
}

