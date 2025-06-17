using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SkinRush.Pages.Admin
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Remove("IsAdmin"); 
            return RedirectToPage("/Admin/Login"); 
        }
    }
}