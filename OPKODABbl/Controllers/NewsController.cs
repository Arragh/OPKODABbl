using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Main;
using OPKODABbl.Service;
using OPKODABbl.ViewModels.News;

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
            News news = await _websiteDB.News.FirstOrDefaultAsync(n => n.Id == newsId);

            // Проверяем, существует ли такая запись в БД
            if (news != null)
            {
                // Если длина заголовка больше отображаемого лимита
                if (news.NewsTitle.Length > 40)
                {
                    // Ограничиваем длину заголовка и всё что сверх меры - переносим на новую отдельную строку
                    string shortTitle = new string(news.NewsTitle.Take(40).ToArray()) + "...";
                    string titleOverflow = "..." + new string(news.NewsTitle.Skip(40).ToArray()) + "<br /><br />";

                    news.NewsTitle = shortTitle;
                    news.NewsBody = titleOverflow + news.NewsBody;

                    // Возврат записи в преобразованном виде
                    ReadNewsViewModel model = new ReadNewsViewModel()
                    {
                        News = news,
                        NewsImages = _websiteDB.NewsImages.Where(i => i.NewsId == newsId)
                    };

                    return View(model);
                }
                else
                {
                    // Возврат записи в исходном виде
                    ReadNewsViewModel model = new ReadNewsViewModel()
                    {
                        News = news,
                        NewsImages = _websiteDB.NewsImages.Where(i => i.NewsId == newsId).OrderBy(i => i.ImageDate)
                    };

                    return View(model);
                }
            }
            else
            {
                return Redirect("/Main/PageNotFound");
            }
        }
        #endregion

        #region Архив новостей
        public async Task<IActionResult> NewsArchive()
        {
            List<News> news = await _websiteDB.News.OrderByDescending(n => n.NewsDate).ToListAsync();
            List<NewsImage> tempImages = await _websiteDB.NewsImages.OrderBy(i => i.ImageDate).ToListAsync();

            Dictionary<Guid, string> previewImages = new Dictionary<Guid, string>();

            foreach (var item in news)
            {
                item.NewsBody = item.NewsBody.DeleteTags();

                NewsImage image = tempImages.FirstOrDefault(i => i.NewsId == item.Id);
                if (image != null)
                {
                    previewImages.Add(image.NewsId, image.ImagePathScaled);
                }
                else
                {
                    previewImages.Add(item.Id, "/images/news_noimage.jpg");
                }
            }

            NewsArchiveViewModel model = new NewsArchiveViewModel()
            {
                News = news,
                PreviewImages = previewImages
            };

            return View(model);
        }
        #endregion

    }
}
