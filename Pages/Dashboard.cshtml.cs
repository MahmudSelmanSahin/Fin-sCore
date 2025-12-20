using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fin_sCore.Pages;

public class DashboardModel : PageModel
{
    public void OnGet()
    {
        // Check if user is authenticated
        var authToken = HttpContext.Session.GetString("AuthToken");
        if (string.IsNullOrEmpty(authToken))
        {
            Response.Redirect("/");
            return;
        }
    }

    public IActionResult OnPostLogout()
    {
        // Clear all session data
        HttpContext.Session.Clear();
        
        // Redirect to login page
        return RedirectToPage("/Index");
    }
}

