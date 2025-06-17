using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkinRush.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinRush.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<CSGOSkin> CSGOSkins { get; set; }
        public List<DotaSkin> DotaSkins { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Проверка авторизации через сессию
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                return RedirectToPage("/Admin/Login");
            }

            CSGOSkins = await _context.CSGOSkins.ToListAsync();
            DotaSkins = await _context.DotaSkins.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteCsgoAsync(int id)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                return RedirectToPage("/Admin/Login");
            }

            var skin = await _context.CSGOSkins.FindAsync(id);
            if (skin != null)
            {
                _context.CSGOSkins.Remove(skin);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteDotaAsync(int id)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                return RedirectToPage("/Admin/Login");
            }

            var skin = await _context.DotaSkins.FindAsync(id);
            if (skin != null)
            {
                _context.DotaSkins.Remove(skin);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
