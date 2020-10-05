using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Models.Gallery;
using OPKODABbl.Service;
using OPKODABbl.ViewModels.Gallery;

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
            List<Gallery> galleries = await _websiteDB.Galleries.OrderByDescending(g => g.GalleryDate).ToListAsync();

            List<GalleryImage> tempImages = await _websiteDB.GalleryImages.OrderBy(i => i.ImageDate).ToListAsync();
            Dictionary<Guid, string> previewImages = new Dictionary<Guid, string>();
            foreach (var item in galleries)
            {
                GalleryImage image = tempImages.FirstOrDefault(i => i.GalleryId == item.Id);
                if (image != null)
                {
                    previewImages.Add(image.GalleryId, image.ImagePathScaled);
                }
                else
                {
                    previewImages.Add(item.Id, "/images/news_noimage.jpg");
                }
            }

            GalleriesViewModel model = new GalleriesViewModel()
            {
                Galleries = galleries,
                PreviewImages = previewImages
            };

            return View(model);
        }
        #endregion

        #region Просмотр галереи
        public async Task<IActionResult> ViewGallery(Guid galleryId)
        {
            Gallery gallery = await _websiteDB.Galleries.FirstOrDefaultAsync(g => g.Id == galleryId);

            if (gallery != null)
            {
                List<GalleryImage> galleryImages = await _websiteDB.GalleryImages.Where(i => i.GalleryId == galleryId).OrderByDescending(i => i.ImageDate).ToListAsync();

                if (gallery.GalleryTitle.Length > 40)
                {
                    // Ограничиваем длину заголовка и всё что сверх меры - переносим на новую отдельную строку
                    string shortTitle = new string(gallery.GalleryTitle.Take(40).ToArray()) + "...";
                    string titleOverflow = "..." + new string(gallery.GalleryTitle.Skip(40).ToArray()) + "<br /><br />";

                    gallery.GalleryTitle = shortTitle;
                    gallery.GalleryDescription = titleOverflow + gallery.GalleryDescription;

                    // Возврат записи в преобразованном виде
                    ViewGalleryViewModel model = new ViewGalleryViewModel()
                    {
                        Gallery = gallery,
                        GalleryImages = galleryImages
                    };

                    return View(model);
                }
                else
                {
                    ViewGalleryViewModel model = new ViewGalleryViewModel()
                    {
                        Gallery = gallery,
                        GalleryImages = galleryImages
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

    }
}
