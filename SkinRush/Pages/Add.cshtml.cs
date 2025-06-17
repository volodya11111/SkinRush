using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkinRush.Models;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkinRush.Pages.Skins
{
    public class AddModel : PageModel
    {
        private readonly AppDbContext _context;

        public AddModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string MarketUrl { get; set; }

        [BindProperty]
        public decimal EditablePrice { get; set; }

        public SkinBase ParsedSkin { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostParseAsync()
        {
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
            catch (System.Exception ex)
            {
                ErrorMessage = $"������: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
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
            catch (System.Exception ex)
            {
                ErrorMessage = $"������ ��� ����������: {ex.Message}";
                return Page();
            }
        }
    }
}
