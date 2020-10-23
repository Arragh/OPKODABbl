using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Account;
using OPKODABbl.Models.Forum;
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

        public MainController(WebsiteContext websiteContext)
        {
            _websiteDB = websiteContext;
        }

        #region Главная страница
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "ОРКОДАВЫ";

            //// Уровень доступа пользователя по умолчанию
            //int userAccessLevel = 1;

            //// Если пользователь аутентифицирован, у него может оказаться более высокий уровень доступа
            //if (User.Identity.IsAuthenticated)
            //{
            //    User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
            //    if (user != null)
            //    {
            //        // Устанавливаем уровень доступа для авторизированного пользователя
            //        userAccessLevel = user.Role.AccessLevel;
            //    }
            //}

            //List<Topic> topics = await _websiteDB.Topics.Where(t => t.Subsection.Section.SectionAccessLevel <= userAccessLevel)
            //                                     .Include(t => t.Replies).ThenInclude(r => r.User)
            //                                     .Include(t => t.Subsection).ToListAsync();

            //topics.SelectMany(t => t.Replies = t.Replies.OrderBy(r => r.ReplyDate).ToList());
            //topics = topics.OrderBy(t => t.Replies.Last().ReplyDate).Take(5).ToList();

            List<News> news = await _websiteDB.News.Include(n => n.NewsImages).OrderByDescending(n => n.NewsDate).Take(3).ToListAsync();
            List<Gallery> galleries = await _websiteDB.Galleries.OrderByDescending(g => g.GalleryDate).Take(3).ToListAsync();

            foreach (var item in news)
            {
                // Удаляем всякие теги из текста
                item.NewsBody = item.NewsBody.DeleteTags();
            }

            IndexViewModel model = new IndexViewModel()
            {
                News = news,
                Galleries = galleries
            };

            return View(model);
        }
        #endregion

        #region 404(Page Not Found)
        public IActionResult PageNotFound()
        {
            ViewBag.Title = "Ошибка 404";

            return View();
        }
        #endregion

    }
}
