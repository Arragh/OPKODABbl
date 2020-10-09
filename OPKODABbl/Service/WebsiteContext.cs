using Microsoft.EntityFrameworkCore;
using OPKODABbl.Models.Gallery;
using OPKODABbl.Models.Main;

namespace OPKODABbl.Service
{
    public class WebsiteContext : DbContext
    {
        public DbSet<News> News { get; set; }
        public DbSet<NewsImage> NewsImages { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }

        public WebsiteContext(DbContextOptions<WebsiteContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
