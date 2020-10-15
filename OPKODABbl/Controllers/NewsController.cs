using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Main;
using OPKODABbl.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Controllers
{
    public class NewsController : Controller
    {
        private readonly WebsiteContext _websiteDB;

        public NewsController(WebsiteContext context)
        {
            _websiteDB = context;
        }

        #region Читать новость
        public async Task<IActionResult> ReadNews(Guid newsId)
        {
            // Ищем такую запись в БД по Id
            News news = await _websiteDB.News.Include(n => n.NewsImages).FirstOrDefaultAsync(n => n.Id == newsId);

            // Проверяем, существует ли такая запись в БД
            if (news != null)
            {
                ViewBag.Title = news.NewsTitle;

                int newsTitleLength = 60;
                // Если длина заголовка больше отображаемого лимита
                if (news.NewsTitle.Length > newsTitleLength)
                {
                    // Ограничиваем длину заголовка и всё что сверх меры - переносим на новую отдельную строку
                    string shortTitle = new string(news.NewsTitle.Take(newsTitleLength).ToArray()) + "...";
                    string titleOverflow = "..." + new string(news.NewsTitle.Skip(newsTitleLength).ToArray()) + "<br /><br />";

                    news.NewsTitle = shortTitle;
                    news.NewsBody = titleOverflow + news.NewsBody;

                    // Возврат записи в преобразованном виде
                    return View(news);
                }

                // Или в изначальном виде
                return View(news);
            }

            // Возврат ошибки 404, если запись не найдена
            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Архив новостей
        public async Task<IActionResult> NewsArchive()
        {
            List<News> news = await _websiteDB.News.Include(n => n.NewsImages).OrderByDescending(n => n.NewsDate).ToListAsync();

            ViewBag.Title = "Архив новостей";

            // Удаляем лишние теги из текста
            foreach (var item in news)
            {
                item.NewsBody = item.NewsBody.DeleteTags();
            }

            return View(news);
        }
        #endregion

    }
}
