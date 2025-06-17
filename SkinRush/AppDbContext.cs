using Microsoft.EntityFrameworkCore;
using SkinRush.Models;

namespace SkinRush
{
    public class AppDbContext : DbContext
    {
        public DbSet<CSGOSkin> CSGOSkins { get; set; }
        public DbSet<DotaSkin> DotaSkins { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}