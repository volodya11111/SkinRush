using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkinRush.Models;
using System.Threading.Tasks;

namespace SkinRush.Pages
{
    public class SkinDetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public SkinDetailsModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Type { get; set; } // "csgo" или "dota"

        public CSGOSkin CSGOSkin { get; set; }
        public DotaSkin DotaSkin { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Type == "csgo")
            {
                CSGOSkin = await _context.CSGOSkins.FindAsync(Id);
                if (CSGOSkin == null) return NotFound();
            }
            else if (Type == "dota")
            {
                DotaSkin = await _context.DotaSkins.FindAsync(Id);
                if (DotaSkin == null) return NotFound();
            }
            else
            {
                return BadRequest("Неизвестный тип скина.");
            }

            return Page();
        }
    }
}
