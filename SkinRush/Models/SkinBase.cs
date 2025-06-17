namespace SkinRush.Models
{
    public abstract class SkinBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string ItemUrl { get; set; }
        public decimal Price { get; set; }
        public string Game { get; set; } // "CSGO" или "DOTA2"
    }

}
