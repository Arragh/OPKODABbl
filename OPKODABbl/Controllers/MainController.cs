using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Areas.Admin.Controllers;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Account;
using OPKODABbl.Models.Gallery;
using OPKODABbl.Models.Main;
using OPKODABbl.Service;
using OPKODABbl.ViewModels.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Controllers
{
    public class MainController : Controller
    {
        private readonly WebsiteContext _websiteDB;
        private readonly UsersContext _usersDB;

        public MainController(WebsiteContext websiteContext, UsersContext usersContext)
        {
            _websiteDB = websiteContext;
            _usersDB = usersContext;
        }

        #region Главная страница
        public async Task<IActionResult> Index()
        {
            List<News> news = await _websiteDB.News.OrderByDescending(n => n.NewsDate).Take(3).ToListAsync();
            List<Gallery> galleries = await _websiteDB.Galleries.OrderByDescending(g => g.GalleryDate).Take(3).ToListAsync();
            List<NewsImage> newsImages = new List<NewsImage>();

            foreach (var item in news)
            {
                // Удаляем всякие теги из текста
                item.NewsBody = item.NewsBody.DeleteTags();

                NewsImage newsImage = await _websiteDB.NewsImages.OrderBy(i => i.ImageDate).FirstOrDefaultAsync(i => i.NewsId == item.Id);
                if (newsImage != null)
                {
                    newsImages.Add(newsImage);
                }
                else
                {
                    newsImages.Add(new NewsImage()
                    {
                        NewsId = item.Id,
                        ImagePathScaled = "/images/news_noimage.jpg"
                    });
                }
            }

            IndexViewModel model = new IndexViewModel()
            {
                News = news,
                NewsImages = newsImages,
                Galleries = galleries
            };

            return View(model);
        }
        #endregion

        #region 404(Page Not Found)
        public IActionResult PageNotFound()
        {
            return View();
        }
        #endregion

    }
}
