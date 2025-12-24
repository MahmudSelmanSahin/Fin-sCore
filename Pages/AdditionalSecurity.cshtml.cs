using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fin_sCore.Pages;

public class AdditionalSecurityModel : PageModel
{
    public string? Identifier { get; set; }

    public void OnGet(string? identifier)
    {
        Identifier = identifier ?? HttpContext.Session.GetString("Identifier");
        
        if (string.IsNullOrEmpty(Identifier))
        {
            Response.Redirect("/");
        }
    }
}

