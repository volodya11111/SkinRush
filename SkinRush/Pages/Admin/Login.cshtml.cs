using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SkinRush.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private const string AdminUsername = "admin";
        private const string AdminPassword = "12345";

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            
        }

        public IActionResult OnPost()
        {
            if (Username == AdminUsername && Password == AdminPassword)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToPage("/Admin/Index");
            }

            ErrorMessage = "Неверный логин или пароль";
            return Page();
        }
    }
}
