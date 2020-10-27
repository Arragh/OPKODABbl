using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Areas.Admin.ViewModels.Gallery;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Gallery;
using OPKODABbl.Service;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace OPKODABbl.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GalleryController : Controller
    {
        private WebsiteContext _websiteDB;
        private IWebHostEnvironment _appEnvironment;

        public GalleryController(WebsiteContext websiteDBContext, IWebHostEnvironment appEnvironment)
        {
            _websiteDB = websiteDBContext;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            List<Gallery> galleries = await _websiteDB.Galleries.ToListAsync();
            SettingsGallery settings = await _websiteDB.SettingsGallery.FirstAsync();

            IndexViewModel model = new IndexViewModel()
            {
                GalleriesPerPage = settings.GalleriesPerPage,
                ImagesPerGallery = settings.ImagesPerGallery,
                MaxImageSize = settings.MaxImageSize,
                ImageResizeQuality = settings.ImageResizeQuality,
                Galleries = galleries
            };

            return View(model);
        }

        #region Сохранение настроек
        public async Task<IActionResult> ApplySettings(IndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                SettingsGallery settings = await _websiteDB.SettingsGallery.FirstAsync();

                settings.GalleriesPerPage = model.GalleriesPerPage;
                settings.ImageResizeQuality = model.ImageResizeQuality;
                settings.MaxImageSize = model.MaxImageSize;
                settings.ImagesPerGallery = model.ImagesPerGallery;

                _websiteDB.SettingsGallery.Update(settings);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Gallery");
        }
        #endregion

        #region Создать галерею [GET]
        [HttpGet]
        public IActionResult CreateGallery()
        {
            return View();
        }
        #endregion

        #region Создать галерею [POST]
        [HttpPost]
        public async Task<IActionResult> CreateGallery(CreateGalleryViewModel model, IFormFile sliderImage)
        {
            // Настройки галереи
            SettingsGallery settings = await _websiteDB.SettingsGallery.FirstAsync();

            int imageSize = 1048576 * settings.MaxImageSize;

            // Проверяем, чтобы обязательно был указан файл с изображением
            if (sliderImage == null || sliderImage.Length == 0)
            {
                ModelState.AddModelError("GallerySliderImage", "Укажите изображение для показа в слайдере.");
            }
            // Проверяем, чтобы входящий файл не превышал установленный максимальный размер
            if (sliderImage != null && sliderImage.Length > imageSize)
            {
                ModelState.AddModelError("GallerySliderImage", $"Файл \"{sliderImage.FileName}\" превышает установленный лимит 2MB.");
            }

            // Проверяем чтобы не случился дубликат имени галереи
            Gallery gallery = await _websiteDB.Galleries.FirstOrDefaultAsync(g => g.GalleryTitle == model.GalleryTitle);
            if (gallery != null)
            {
                ModelState.AddModelError("GalleryTitle", "Галерея с таким именем уже существует");
            }

            // Если на пути проблем не возникло, то создаем галерею
            if (ModelState.IsValid)
            {
                // Создаем новый объект класса FileInfo из полученного изображения для дальнейшей обработки
                FileInfo imgFile = new FileInfo(sliderImage.FileName);
                // Приводим расширение файла к нижнему регистру
                string imgExtension = imgFile.Extension.ToLower();
                // Генерируем новое имя для файла
                string newFileName = Guid.NewGuid() + imgExtension;
                // Пути сохранения файла
                string sliderDirectory = "/uploadedfiles/gallery/images/slider/";
                // Если такой директории не существует, то создаем её
                if (!Directory.Exists(_appEnvironment.WebRootPath + sliderDirectory))
                {
                    Directory.CreateDirectory(_appEnvironment.WebRootPath + sliderDirectory);
                }
                // Путь для слайд-изображения
                string pathSliderImage = sliderDirectory + newFileName;

                // В операторе try/catch делаем уменьшенную копию изображения.
                // Если входным файлом окажется не изображение, нас перекинет в блок CATCH и выведет сообщение об ошибке
                try
                {
                    // Создаем объект класса SixLabors.ImageSharp.Image и грузим в него полученное изображение
                    using (Image image = Image.Load(sliderImage.OpenReadStream()))
                    {
                        // Создаем уменьшенную копию и обрезаем её
                        var clone = image.Clone(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Crop,
                            Size = new Size(1056, 220)
                        }));
                        // Сохраняем уменьшенную копию
                        await clone.SaveAsync(_appEnvironment.WebRootPath + pathSliderImage, new JpegEncoder { Quality = settings.ImageResizeQuality });
                    }
                }
                // Если вдруг что-то пошло не так (например, на вход подало не картинку), то выводим сообщение об ошибке
                catch
                {
                    // Создаем сообщение об ошибке для вывода пользователю
                    ModelState.AddModelError("GallerySliderImage", $"Файл {sliderImage.FileName} имеет неверный формат.");

                    // Возвращаем модель с сообщением об ошибке в представление
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.GalleryDescription))
                {
                    model.GalleryDescription = "";
                }

                gallery = new Gallery()
                {
                    Id = Guid.NewGuid(),
                    GalleryTitle = model.GalleryTitle,
                    GalleryDescription = model.GalleryDescription.SpecSymbolsToView(),
                    GalleryDate = DateTime.Now,
                    GalleryUserName = User.Identity.Name,
                    GallerySliderImage = pathSliderImage
                };

                await _websiteDB.Galleries.AddAsync(gallery);
                await _websiteDB.SaveChangesAsync();

                return RedirectToAction("Index", "Gallery");
            }

            // Возврат модели при неудачной валидации
            return View(model);
        }
        #endregion

        #region Просмотр галереи / Удаление(Добавление) изображений [GET]
        [HttpGet]
        public async Task<IActionResult> ViewGallery(Guid galleryId, string imageToDeleteName = null)
        {
            // Если есть изображение, которое надо удалить, заходим в тело условия
            if (imageToDeleteName != null)
            {
                // Создаем экземпляр класса картинки и присваиваем ему данные из БД
                GalleryImage galleryImage = await _websiteDB.GalleryImages.FirstOrDefaultAsync(i => i.ImageName == imageToDeleteName);

                // Делаем еще одну проверку. Лучше перебдеть. Если все ок, заходим в тело условия и удаляем изображения
                if (galleryImage != null)
                {
                    // Исходные (полноразмерные) изображения
                    FileInfo imageOriginal = new FileInfo(_appEnvironment.WebRootPath + galleryImage.ImagePathOriginal);
                    if (imageOriginal.Exists)
                    {
                        imageOriginal.Delete();
                    }
                    // И их уменьшенные копии
                    FileInfo imageScaled = new FileInfo(_appEnvironment.WebRootPath + galleryImage.ImagePathScaled);
                    if (imageScaled.Exists)
                    {
                        imageScaled.Delete();
                    }
                    // Удаляем информацию об изображениях из БД и сохраняем
                    _websiteDB.GalleryImages.Remove(galleryImage);
                    await _websiteDB.SaveChangesAsync();
                }
            }

            // Выбираем галерею из БД
            Gallery gallery = await _websiteDB.Galleries.FirstOrDefaultAsync(g => g.Id == galleryId);
            if (gallery != null)
            {
                // Создаем список изображений из БД, закрепленных за выбранной галереей
                List<GalleryImage> images = await _websiteDB.GalleryImages.Where(i => i.GalleryId == galleryId).OrderByDescending(i => i.ImageDate).ToListAsync();

                // Создаем модель для передачи в представление и присваиваем значения
                ViewGalleryViewModel model = new ViewGalleryViewModel()
                {
                    GalleryTitle = gallery.GalleryTitle,
                    GalleryDescription = gallery.GalleryDescription,
                    GalleryImages = images,
                    // Скрытые поля
                    GalleryId = galleryId,
                    GalleryDate = gallery.GalleryDate,
                    UserName = gallery.GalleryUserName,
                    ImagesCount = images.Count
                };

                // Передаем модель в представление
                return View(model);
            }
            else
            {
                return Redirect("/Main/PageNotFound");
            }
        }
        #endregion

        #region Просмотр галереи / Добавление изображений [POST]
        [HttpPost]
        public async Task<IActionResult> ViewGallery(ViewGalleryViewModel model, IFormFileCollection uploads)
        {
            // Настройки галереи
            SettingsGallery settings = await _websiteDB.SettingsGallery.FirstAsync();

            int imageSize = 1048576 * settings.MaxImageSize;
            int imagesPerGallery = settings.ImagesPerGallery;

            // Выбираем все изображения, находящиеся в данной галерее
            List<GalleryImage> images = await _websiteDB.GalleryImages.Where(i => i.GalleryId == model.GalleryId).OrderByDescending(i => i.ImageDate).ToListAsync();

            // Проверяем, не превышает ли количество загружаемых изображений допустимый лимит
            if (uploads.Count > imagesPerGallery - images.Count)
            {
                ModelState.AddModelError("GalleryImage", $"Превышен лимит изображений для галереи.");
                //ModelState.AddModelError("GalleryImage", $"Вы пытаетесь загрузить {uploads.Count} изображений. Лимит галереи {imagesPerGallery} изображений. Вы можете загрузить еще {imagesPerGallery - images.Count} изображений.");
            }
            // Если всё в порядке, заходим в ELSE
            else
            {
                // Проверяем, чтобы размер файлов не превышал заданный объем
                foreach (var file in uploads)
                {
                    if (file == null || file.Length == 0)
                    {
                        ModelState.AddModelError("GalleryImage", $"Произошла ошибка при обработке файла \"{file.FileName}\".");
                        break;
                    }
                    if (file.Length > imageSize)
                    {
                        ModelState.AddModelError("GalleryImage", $"Файл \"{file.FileName}\" превышает установленный лимит {settings.MaxImageSize}MB.");
                        break;
                    }
                }
            }

            // Если все в порядке, заходим в тело условия
            if (ModelState.IsValid)
            {
                // Далее начинаем обработку загружаемых изображений
                List<GalleryImage> galleryImages = new List<GalleryImage>();
                foreach (var uploadedImage in uploads)
                {
                    // Создаем новый объект класса FileInfo из полученного изображения для дальнейшей обработки
                    FileInfo imgFile = new FileInfo(uploadedImage.FileName);
                    // Приводим расширение к нижнему регистру (если оно было в верхнем)
                    string imgExtension = imgFile.Extension.ToLower();
                    // Генерируем новое имя для файла
                    string newFileName = Guid.NewGuid() + imgExtension;
                    // Пути сохранения файла
                    string originalDirectory = "/uploadedfiles/gallery/images/original/";
                    string scaledDirectory = "/uploadedfiles/gallery/images/scaled/";
                    string pathOriginal = originalDirectory + newFileName; // изображение исходного размера
                    string pathScaled = scaledDirectory + newFileName; // уменьшенное изображение

                    // Если такой директории не существует, то создаем её
                    if (!Directory.Exists(_appEnvironment.WebRootPath + originalDirectory))
                    {
                        Directory.CreateDirectory(_appEnvironment.WebRootPath + originalDirectory);
                    }
                    if (!Directory.Exists(_appEnvironment.WebRootPath + scaledDirectory))
                    {
                        Directory.CreateDirectory(_appEnvironment.WebRootPath + scaledDirectory);
                    }

                    // В операторе try/catch делаем уменьшенную копию изображения.
                    // Если входным файлом окажется не изображение, нас перекинет в блок CATCH и выведет сообщение об ошибке
                    try
                    {
                        // Создаем объект класса SixLabors.ImageSharp.Image и грузим в него полученное изображение
                        using (Image image = Image.Load(uploadedImage.OpenReadStream()))
                        {
                            // Создаем уменьшенную копию и обрезаем её
                            var clone = image.Clone(x => x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Crop,
                                Size = new Size(300, 169)
                            }));
                            // Сохраняем уменьшенную копию
                            await clone.SaveAsync(_appEnvironment.WebRootPath + pathScaled, new JpegEncoder { Quality = settings.ImageResizeQuality });
                            // Сохраняем исходное изображение
                            await image.SaveAsync(_appEnvironment.WebRootPath + pathOriginal);
                        }
                    }
                    // Если вдруг что-то пошло не так (например, на вход подало не картинку), то выводим сообщение об ошибке
                    catch
                    {
                        // Создаем сообщение об ошибке для вывода пользователю
                        ModelState.AddModelError("GalleryImage", $"Файл {uploadedImage.FileName} имеет неверный формат.");

                        // Удаляем только что созданные файлы (если ошибка возникла не на первом файле и некоторые уже были загружены на сервер)
                        foreach (var image in galleryImages)
                        {
                            // Исходные (полноразмерные) изображения
                            FileInfo imageOriginal = new FileInfo(_appEnvironment.WebRootPath + image.ImagePathOriginal);
                            if (imageOriginal.Exists)
                            {
                                imageOriginal.Delete();
                            }
                            // И их уменьшенные копии
                            FileInfo imageScaled = new FileInfo(_appEnvironment.WebRootPath + image.ImagePathScaled);
                            if (imageScaled.Exists)
                            {
                                imageScaled.Delete();
                            }
                        }
                        // Возвращаем модель с сообщением об ошибке в представление
                        return View(model);
                    }

                    // Создаем объект класса GalleryImage со всеми параметрами
                    GalleryImage galleryImage = new GalleryImage()
                    {
                        Id = Guid.NewGuid(),
                        ImageName = newFileName,
                        ImagePathOriginal = pathOriginal,
                        ImagePathScaled = pathScaled,
                        GalleryId = model.GalleryId,
                        ImageDate = DateTime.Now
                    };
                    // Добавляем объект galleryImage в список galleryImages
                    galleryImages.Add(galleryImage);
                }

                // Если в процессе выполнения не возникло ошибок, сохраняем всё в БД
                if (galleryImages != null && galleryImages.Count > 0)
                {
                    await _websiteDB.GalleryImages.AddRangeAsync(galleryImages);
                    await _websiteDB.SaveChangesAsync();
                }

                // Выводим обновленную модель в представление
                return RedirectToAction("ViewGallery", "Gallery", new { galleryId = model.GalleryId });
            }

            // В случае, если произошла ошибка валидации, требуется заново присвоить список изображений и счетчик для возвращаемой модели
            // При перегонке модели из гет в пост, теряется список с изображениями. Причина пока не ясна, поэтому сделал такой костыль
            // Счетчик соответственно тоже обнулялся, поэтому его тоже приходится переназначать заново
            model.GalleryImages = images;
            model.ImagesCount = images.Count;

            // Возврат модели в представление в случае, если запорится валидация
            return View(model);
        }
        #endregion

        #region Редактировать галерею [GET]
        [HttpGet]
        public async Task<IActionResult> EditGallery(Guid galleryId)
        {
            // Находим галерею по Id
            Gallery gallery = await _websiteDB.Galleries.FirstOrDefaultAsync(g => g.Id == galleryId);

            if (gallery != null)
            {
                // Создаем модель для передачи в представление
                EditGalleryViewModel model = new EditGalleryViewModel()
                {
                    GalleryId = galleryId,
                    GalleryTitle = gallery.GalleryTitle,
                    GalleryDescription = gallery.GalleryDescription.SpecSymbolsToEdit(),
                    GalleryPreviewImage = gallery.GallerySliderImage
                };

                // Возвращаем модель в представление
                return View(model);
            }
            else
            {
                return Redirect("/Main/PageNotFound");
            }
        }
        #endregion

        #region Редактировать галерею [POST]
        [HttpPost]
        public async Task<IActionResult> EditGallery(EditGalleryViewModel model, IFormFile sliderImage)
        {
            // Настройки галереи
            SettingsGallery settings = await _websiteDB.SettingsGallery.FirstAsync();

            int imageSize = 1048576 * settings.MaxImageSize;

            // Если размер превью-изображения превышает установленный лимит, генерируем ошибку модели
            if (sliderImage != null && sliderImage.Length > imageSize)
            {
                ModelState.AddModelError("GalleryPreviewImage", $"Файл \"{sliderImage.FileName}\" превышает установленный лимит {settings.MaxImageSize}MB.");
            }

            // Проверяем, чтобы такая галерея существовала в БД
            Gallery gallery = await _websiteDB.Galleries.FirstOrDefaultAsync(g => g.Id == model.GalleryId);
            if (gallery == null)
            {
                return Redirect("/Main/PageNotFound");
            }

            // Если ошибок не возникло, заходим в тело условия
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.GalleryDescription))
                {
                    model.GalleryDescription = "";
                }

                // Если исходный файл не равен NULL и его размер больше 0, заходим в тело условия
                if (sliderImage != null && sliderImage.Length > 0)
                {
                    // Создаем новый объект класса FileInfo из полученного изображения для дальнейшей обработки
                    FileInfo imgFile = new FileInfo(sliderImage.FileName);
                    // Приводим расширение к нижнему регистру (если оно было в верхнем)
                    string imgExtension = imgFile.Extension.ToLower();
                    // Генерируем новое имя для файла
                    string newFileName = Guid.NewGuid() + imgExtension;
                    // Пути сохранения файла
                    string sliderDirectory = "/uploadedfiles/gallery/images/slider/";
                    // Пути сохранения файла
                    string pathSliderImage = sliderDirectory + newFileName; // уменьшенное изображение
                    
                    // Если такой директории не существует, то создаем её
                    if (!Directory.Exists(_appEnvironment.WebRootPath + sliderDirectory))
                    {
                        Directory.CreateDirectory(_appEnvironment.WebRootPath + sliderDirectory);
                    }

                    // В операторе try/catch делаем уменьшенную копию изображения.
                    // Если входным файлом окажется не изображение, нас перекинет в блок CATCH и выведет сообщение об ошибке
                    try
                    {
                        // Создаем объект класса SixLabors.ImageSharp.Image и грузим в него полученное изображение
                        using (Image image = Image.Load(sliderImage.OpenReadStream()))
                        {
                            // Создаем уменьшенную копию и обрезаем её
                            var clone = image.Clone(x => x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Crop,
                                Size = new Size(1056, 220)
                            }));
                            // Сохраняем уменьшенную копию
                            await clone.SaveAsync(_appEnvironment.WebRootPath + pathSliderImage, new JpegEncoder { Quality = settings.ImageResizeQuality });
                        }
                    }
                    // Если вдруг что-то пошло не так (например, на вход подало не картинку), то выводим сообщение об ошибке
                    catch
                    {
                        // Создаем сообщение об ошибке для вывода пользователю
                        ModelState.AddModelError("GalleryPreviewImage", $"Файл {sliderImage.FileName} имеет неверный формат.");

                        // Возвращаем модель с сообщением об ошибке в представление
                        return View(model);
                    }

                    // Удаляем предыдущее изображения для слайдера
                    if (gallery.GallerySliderImage != null)
                    {
                        FileInfo imageToDelete = new FileInfo(_appEnvironment.WebRootPath + gallery.GallerySliderImage);
                        if (imageToDelete.Exists)
                        {
                            imageToDelete.Delete();
                        }
                    }

                    // Обновляем значения на полученные с формы
                    gallery.GalleryTitle = model.GalleryTitle;
                    gallery.GalleryDescription = model.GalleryDescription.SpecSymbolsToView();
                    gallery.GallerySliderImage = pathSliderImage;


                    // Сохраняем изменения в БД
                    _websiteDB.Galleries.Update(gallery);
                    await _websiteDB.SaveChangesAsync();

                    return RedirectToAction("Index", "Gallery");
                }
                // Если не была выбрана картинка для превью, заходим в блок ELSE
                else
                {
                    // Обновляем значения на полученные с формы
                    gallery.GalleryTitle = model.GalleryTitle;
                    gallery.GalleryDescription = model.GalleryDescription.SpecSymbolsToView();

                    // Сохраняем изменения в БД
                    _websiteDB.Galleries.Update(gallery);
                    await _websiteDB.SaveChangesAsync();

                    return RedirectToAction("Index", "Gallery");
                }
            }

            // Возврат модели при неудачной валидации
            return View(model);
        }
        #endregion

        #region Удалить галерею [GET]
        public async Task<IActionResult> DeleteGallery(Guid galleryId)
        {
            Gallery gallery = await _websiteDB.Galleries.FirstOrDefaultAsync(g => g.Id == galleryId);
            if (gallery != null)
            {
                List<GalleryImage> galleryImages = await _websiteDB.GalleryImages.Where(i => i.GalleryId == galleryId).ToListAsync();

                if (galleryImages.Count > 0)
                {
                    foreach (var galleryImage in galleryImages)
                    {
                        // Делаем еще одну проверку. Лучше перебдеть. Если все ок, заходим в тело условия и удаляем изображения
                        if (galleryImage != null)
                        {
                            // Исходные (полноразмерные) изображения
                            FileInfo imageOriginal = new FileInfo(_appEnvironment.WebRootPath + galleryImage.ImagePathOriginal);
                            if (imageOriginal.Exists)
                            {
                                imageOriginal.Delete();
                            }
                            // И их уменьшенные копии
                            FileInfo imageScaled = new FileInfo(_appEnvironment.WebRootPath + galleryImage.ImagePathScaled);
                            if (imageScaled.Exists)
                            {
                                imageScaled.Delete();
                            }
                        }
                    }
                }

                // Удаляем слайдер-изображение
                if (gallery.GallerySliderImage != null)
                {
                    FileInfo sliderImage = new FileInfo(_appEnvironment.WebRootPath + gallery.GallerySliderImage);
                    if (sliderImage.Exists)
                    {
                        sliderImage.Delete();
                    }
                }

                // Удаляем информацию об изображениях из БД
                _websiteDB.GalleryImages.RemoveRange(galleryImages);
                // Удаляем галерею из БД
                _websiteDB.Galleries.Remove(gallery);
                // Сохраняем
                await _websiteDB.SaveChangesAsync();

                return RedirectToAction("Index", "Gallery");
            }
            else
            {
                return Redirect("/Main/PageNotFound");
            }
        }
        #endregion

    }
}
