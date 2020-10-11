using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Models.Gallery;
using OPKODABbl.Service;

namespace OPKODABbl.Controllers
{
    public class GalleryController : Controller
    {
        private readonly WebsiteContext _websiteDB;

        public GalleryController(WebsiteContext context)
        {
            _websiteDB = context;
        }

        #region Список галерей
        public async Task<IActionResult> Galleries()
        {
            List<Gallery> galleries = await _websiteDB.Galleries.Include(g => g.GalleryImages).OrderByDescending(g => g.GalleryDate).ToListAsync();

            return View(galleries);
        }
        #endregion

        #region Просмотр галереи
        public async Task<IActionResult> ViewGallery(Guid galleryId)
        {
            Gallery gallery = await _websiteDB.Galleries.Include(g => g.GalleryImages).FirstOrDefaultAsync(g => g.Id == galleryId);

            if (gallery != null)
            {
                // Урезание заголовка до 40 символов
                if (gallery.GalleryTitle.Length > 40)
                {
                    // Ограничиваем длину заголовка и всё что сверх меры - переносим на новую отдельную строку
                    string shortTitle = new string(gallery.GalleryTitle.Take(40).ToArray()) + "...";
                    string titleOverflow = "..." + new string(gallery.GalleryTitle.Skip(40).ToArray()) + "<br /><br />";

                    gallery.GalleryTitle = shortTitle;
                    gallery.GalleryDescription = titleOverflow + gallery.GalleryDescription;

                    // Возврат записи в преобразованном виде
                    return View(gallery);
                }

                // Или в изначальном виде
                return View(gallery);
            }

            return Redirect("/Main/PageNotFound");

        }
        #endregion

    }
}
