using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SkinRush.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class SteamSkinService
{
    private readonly HttpClient _http = new();

    private class GenericSkin:SkinBase { }
    public async Task<SkinBase> GetSkinFromMarketUrlAsync(string url)
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless"); 

        using var driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl(url);

        Thread.Sleep(1000); 

        var htmlSelenium = driver.PageSource;
        
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(htmlSelenium);


        string html = await _http.GetStringAsync(url);

        var m = Regex.Match(url, @"market/listings/(\d+)/(.+)$");
        if (!m.Success)
            throw new Exception("Не удалось извлечь appid и market_hash_name из URL");

        string appid = m.Groups[1].Value;
        string marketHashNameEncoded = m.Groups[2].Value;
        string marketHashName = Uri.UnescapeDataString(marketHashNameEncoded);

        var assetsMatch = Regex.Match(html, @"var g_rgAssets = (.+?);\n", RegexOptions.Singleline);
        if (!assetsMatch.Success)
            throw new Exception("Не найден JSON g_rgAssets в HTML");
       
        

        string pattern = @"<div\s+id=""largeiteminfo""[^>]*>(.*?)</div>";

        var node = doc.DocumentNode.SelectSingleNode("//div[@id='largeiteminfo']");
 
        string assetsJson = assetsMatch.Groups[1].Value;
        var assets = JObject.Parse(assetsJson);

        var appAssets = assets[appid] as JObject;
        if (appAssets == null)
            throw new Exception("Не найден appAssets для appid: " + appid);

        SkinBase skin = null;

        foreach (var context in appAssets.Properties())
        {
            var contextItems = context.Value as JObject;
            if (contextItems == null)
                continue;

            foreach (var itemProp in contextItems.Properties())
            {
                var item = itemProp.Value as JObject;
                if (item == null)
                    continue;

                var name = (string)item["market_hash_name"];
                if (name != null && name.Equals(marketHashName, StringComparison.OrdinalIgnoreCase))
                {
                    string game = appid switch
                    {
                        "730" => "CS2",
                        "570" => "Dota 2",
                        _ => "Unknown"
                    };

                    string iconUrlPart = (string)item["icon_url"];
                    string fullImageUrl = !string.IsNullOrEmpty(iconUrlPart)
                        ? $"https://steamcommunity-a.akamaihd.net/economy/image/{iconUrlPart}"
                        : "";

                    if (game == "CS2")
                    {
                        var csSkin = new CSGOSkin
                        {
                            Name = name,
                            ItemUrl = url,
                            ImageUrl = fullImageUrl,
                            Game = game,
                            Type = "",
                            Exterior = "",
                        };

                        csSkin.Type = node.SelectSingleNode(".//div[@id='largeiteminfo_item_type']")?.InnerText.Trim();

                        var descriptors = node.SelectNodes(".//div[@id='largeiteminfo_item_descriptors']//div[@class='descriptor']");
                        if (descriptors != null)
                        {
                            foreach (var desc in descriptors)
                            {
                                var text = desc.InnerText.Trim();

                                if (text.StartsWith("Износ:"))
                                {
                                    csSkin.Exterior = text.Replace("Износ:", "").Trim(); 
                                }
                            }
                        }

                        skin = csSkin;
                    }
                    else if (game == "Dota 2")
                    {
                        var dotaSkin = new DotaSkin
                        {
                            Name = name,
                            ItemUrl = url,
                            ImageUrl = fullImageUrl,
                            Game = game,
                            Hero = "",
                            Type = "",
                            Price = 0
                        };

                        dotaSkin.Type = node.SelectSingleNode(".//div[@id='largeiteminfo_item_type']")?.InnerText.Trim();

                        var descriptors = node.SelectNodes(".//div[@id='largeiteminfo_item_descriptors']//div[@class='descriptor']");
                        dotaSkin.Hero = descriptors?
                            .FirstOrDefault(d => d.InnerText.Trim().StartsWith("Используется:"))?
                            .InnerText.Replace("Используется:", "").Trim();


                        skin = dotaSkin;
                    }
                    else
                    {
                        skin = new GenericSkin
                        {
                            Name = name,
                            ItemUrl = url,
                            ImageUrl = fullImageUrl,
                            Game = game,
                            Price = 0
                        };
                    }

                    break;
                }
            }

            if (skin != null)
                break;
        }

        if (skin == null)
            throw new Exception("Предмет не найден в JSON данных Steam");

        string priceUrl = $"https://steamcommunity.com/market/priceoverview/?currency=1&appid={appid}&market_hash_name={Uri.EscapeDataString(marketHashName)}";
        string priceJson = await _http.GetStringAsync(priceUrl);
        var priceData = JObject.Parse(priceJson);

        if (priceData["success"]?.Value<bool>() == true)
        {
            string priceStr = (string)priceData["median_price"] ?? (string)priceData["lowest_price"];
            if (!string.IsNullOrEmpty(priceStr))
            {
                string cleanPrice = priceStr.Replace("$", "").Replace("₽", "").Replace(",", ".").Trim();
                if (decimal.TryParse(cleanPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                {
                    skin.Price = price;
                }
                else
                {
                    skin.Price = 0;
                }
            }
        }
        else
        {
            skin.Price = 0;
        }

        return skin;
    }
}
