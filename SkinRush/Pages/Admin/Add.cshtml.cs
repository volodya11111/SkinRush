using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkinRush.Models;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkinRush.Pages.Admin
{
    public class AddModel : PageModel
    {
        private readonly AppDbContext _context;

        public AddModel(AppDbContext context)
        {
            _context = context;
        }
        [BindProperty] public string ManualGame { get; set; }
        [BindProperty] public string ManualName { get; set; }
        [BindProperty] public decimal ManualPrice { get; set; }
        [BindProperty] public string ManualImageUrl { get; set; }
        [BindProperty] public string ManualItemUrl { get; set; }

        [BindProperty] public string ManualCsgoType { get; set; }
        [BindProperty] public string ManualCsgoExterior { get; set; }

        [BindProperty] public string ManualDotaHero { get; set; }
        [BindProperty] public string ManualDotaType { get; set; }

        [BindProperty]
        public string MarketUrl { get; set; }

        [BindProperty]
        public decimal EditablePrice { get; set; }

        public SkinBase ParsedSkin { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                return RedirectToPage("/Admin/Login");
            }
            return Page();
        }
        public async Task<IActionResult> OnPostParseAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                return RedirectToPage("/Admin/Login");
            }

            if (string.IsNullOrWhiteSpace(MarketUrl))
            {
                ErrorMessage = "������� ������ �� �������.";
                return Page();
            }

            try
            {
                var service = new SteamSkinService();
                ParsedSkin = await service.GetSkinFromMarketUrlAsync(MarketUrl);

                if (ParsedSkin == null)
                {
                    ErrorMessage = "�� ������� �������� ������ � �����.";
                    return Page();
                }

                EditablePrice = ParsedSkin.Price;

                // ��������� ��������������� ���� � ��� � TempData
                TempData["ParsedSkin"] = JsonSerializer.Serialize(ParsedSkin, ParsedSkin.GetType());
                TempData["SkinType"] = ParsedSkin.GetType().Name;

                // ����� TempData �� ��������� ����� ����� �������
                TempData.Keep("ParsedSkin");
                TempData.Keep("SkinType");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"������: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostManualAddAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                return RedirectToPage("/Admin/Login");
            }
            try
            {
                if (string.IsNullOrWhiteSpace(ManualGame) || string.IsNullOrWhiteSpace(ManualName) || ManualPrice <= 0 || string.IsNullOrWhiteSpace(ManualImageUrl))
                {
                    ErrorMessage = "����������, ��������� ��� ������������ ����.";
                    return Page();
                }

                if (ManualGame == "CSGO")
                {
                    var skin = new CSGOSkin
                    {
                        Name = ManualName,
                        Price = ManualPrice,
                        ImageUrl = ManualImageUrl,
                        ItemUrl = ManualItemUrl,
                        Game = "CS:GO",
                        Type = ManualCsgoType,
                        Exterior = ManualCsgoExterior
                    };
                    _context.CSGOSkins.Add(skin);
                }
                else if (ManualGame == "Dota")
                {
                    var skin = new DotaSkin
                    {
                        Name = ManualName,
                        Price = ManualPrice,
                        ImageUrl = ManualImageUrl,
                        ItemUrl = ManualItemUrl,
                        Game = "Dota 2",
                        Hero = ManualDotaHero,
                        Type = ManualDotaType
                    };
                    _context.DotaSkins.Add(skin);
                }
                else
                {
                    ErrorMessage = "����������� ����.";
                    return Page();
                }

                await _context.SaveChangesAsync();
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"������ ��� ���������� �������: {ex.Message}";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostSaveAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                return RedirectToPage("/Admin/Login");
            }
            try
            {
                if (!TempData.ContainsKey("ParsedSkin") || !TempData.ContainsKey("SkinType"))
                {
                    ErrorMessage = "���� �� ������. ���������� �����.";
                    return Page();
                }

                var skinType = TempData["SkinType"] as string;
                var skinJson = TempData["ParsedSkin"] as string;

                if (string.IsNullOrEmpty(skinJson) || string.IsNullOrEmpty(skinType))
                {
                    ErrorMessage = "���� �� ������.";
                    return Page();
                }

                SkinBase skin = skinType switch
                {
                    nameof(CSGOSkin) => JsonSerializer.Deserialize<CSGOSkin>(skinJson),
                    nameof(DotaSkin) => JsonSerializer.Deserialize<DotaSkin>(skinJson),
                    _ => null
                };

                if (skin == null)
                {
                    ErrorMessage = "�� ������� ������������ ������ �����.";
                    return Page();
                }

                // ��������� ���� �� ���� �����
                if (EditablePrice > 0)
                    skin.Price = EditablePrice;

                if (skin is CSGOSkin csgoSkin)
                    _context.CSGOSkins.Add(csgoSkin);
                else if (skin is DotaSkin dotaSkin)
                    _context.DotaSkins.Add(dotaSkin);
                else
                {
                    ErrorMessage = "����������� ��� �����.";
                    return Page();
                }

                await _context.SaveChangesAsync();

                // ����� ���������� ������� TempData, ����� �� ����������� ��������� ����
                TempData.Remove("ParsedSkin");
                TempData.Remove("SkinType");

                return RedirectToPage(); // �������� ����� � �������� ��������
            }
            catch (Exception ex)
            {
                ErrorMessage = $"������ ��� ����������: {ex.Message}";
                return Page();
            }
        }
    }
}
