using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkinRush.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinRush.Pages
{
    public class SkinsModel : PageModel
    {
        private readonly AppDbContext _context;

        public SkinsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<CSGOSkin> CSGOSkins { get; set; } = new();
        public List<DotaSkin> DotaSkins { get; set; } = new();

        public async Task OnGetAsync()
        {
            CSGOSkins = await _context.CSGOSkins.ToListAsync();
            DotaSkins = await _context.DotaSkins.ToListAsync();
        }
    }
}