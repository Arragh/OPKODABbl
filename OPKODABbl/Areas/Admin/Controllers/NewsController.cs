using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Areas.Admin.ViewModels.News;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Main;
using OPKODABbl.Service;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace OPKODABbl.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NewsController : Controller
    {
        private WebsiteContext _websiteDB;
        private IWebHostEnvironment _appEnvironment;

        public NewsController(WebsiteContext websiteDBContext, IWebHostEnvironment appEnvironment)
        {
            _websiteDB = websiteDBContext;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _websiteDB.News.OrderByDescending(n => n.NewsDate).ToListAsync());
        }

        #region Создать новость [GET]
        [HttpGet]
        public IActionResult CreateNews()
        {
            return View();
        }
        #endregion

        #region Создать новость [POST]
        [HttpPost]
        public async Task<IActionResult> CreateNews(CreateNewsViewModel model, IFormFileCollection uploads)
        {
            // Объем изображения в мегабайтах
            int imageSize = 1048576 * 2;

            if (uploads.Count > 3)
            {
                ModelState.AddModelError("NewsImage", $"Нельзя загрузить больше 3 изображений");
            }
            else
            {
                // Проверяем, чтобы размер файлов не превышал заданный объем
                foreach (var file in uploads)
                {
                    if (file.Length > imageSize)
                    {
                        ModelState.AddModelError("NewsImage", $"Файл \"{file.FileName}\" превышает установленный лимит 2MB.");
                        break;
                    }
                }
            }

            // Если все в порядке, заходим в тело условия
            if (ModelState.IsValid)
            {
                // Создаем экземпляр класса News и присваиваем ему значения
                News news = new News()
                {
                    Id = Guid.NewGuid(),
                    NewsTitle = model.NewsTitle,
                    NewsBody = model.NewsBody.SpecSymbolsToView(),
                    NewsDate = DateTime.Now,
                    NewsUserName = User.Identity.Name
                };

                // Далее начинаем обработку изображений
                List<NewsImage> newsImages = new List<NewsImage>();
                foreach (var uploadedImage in uploads)
                {
                    // Если размер входного файла больше 0, заходим в тело условия
                    if (uploadedImage.Length > 0)
                    {
                        // Создаем новый объект класса FileInfo из полученного изображения для дальнейшей обработки
                        FileInfo imgFile = new FileInfo(uploadedImage.FileName);
                        // Приводим расширение к нижнему регистру (если оно было в верхнем)
                        string imgExtension = imgFile.Extension.ToLower();
                        // Генерируем новое имя для файла
                        string newFileName = Guid.NewGuid() + imgExtension;
                        // Пути сохранения файла
                        string originalDirectory = "/uploadedfiles/news/images/original/";
                        string scaledDirectory = "/uploadedfiles/news/images/scaled/";
                        string pathOriginal = originalDirectory + newFileName; // изображение исходного размера
                        string pathScaled = scaledDirectory + newFileName; // уменьшенное изображение

                        // Если такие директории не созданы, то создаем их
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
                                await clone.SaveAsync(_appEnvironment.WebRootPath + pathScaled, new JpegEncoder { Quality = 50 });
                                // Сохраняем исходное изображение
                                await image.SaveAsync(_appEnvironment.WebRootPath + pathOriginal);
                            }

                        }
                        // Если вдруг что-то пошло не так (например, на вход подало не картинку), то выводим сообщение об ошибке
                        catch
                        {
                            // Создаем сообщение об ошибке для вывода пользователю
                            ModelState.AddModelError("NewsImage", $"Ошибка при загрузке файла {uploadedImage.FileName}. Обратитесь к администратору сайта.");

                            // Удаляем только что созданные файлы (если ошибка возникла не на первом файле)
                            foreach (var image in newsImages)
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

                        // Создаем объект класса NewsImage со всеми параметрами
                        NewsImage newsImage = new NewsImage()
                        {
                            Id = Guid.NewGuid(),
                            ImageName = newFileName,
                            ImagePathOriginal = pathOriginal,
                            ImagePathScaled = pathScaled,
                            NewsId = news.Id,
                            ImageDate = DateTime.Now
                        };
                        // Добавляем объект newsImage в список newsImages
                        newsImages.Add(newsImage);
                    }
                }

                // Если в процессе выполнения не возникло ошибок, сохраняем всё в БД
                if (newsImages != null && newsImages.Count > 0)
                {
                    await _websiteDB.NewsImages.AddRangeAsync(newsImages);
                }
                await _websiteDB.News.AddAsync(news);
                await _websiteDB.SaveChangesAsync();

                // Редирект на главную страницу
                return RedirectToAction("Index", "News");
            }

            // Возврат модели в представление в случае, если запорится валидация
            return View(model);
        }
        #endregion

        #region Редактировать новость [GET]
        [HttpGet]
        public async Task<IActionResult> EditNews(Guid newsId, string imageToDeleteName = null)
        {
            // Находим запись в БД
            News news = await _websiteDB.News.FirstOrDefaultAsync(n => n.Id == newsId);

            // И проверяем, чтобы она существовала
            if (news != null)
            {
                // Если есть изображение, которое надо удалить, заходим в тело условия
                if (imageToDeleteName != null)
                {
                    // Создаем экземпляр класса картинки и присваиваем ему данные из БД
                    NewsImage newsImage = await _websiteDB.NewsImages.FirstOrDefaultAsync(i => i.ImageName == imageToDeleteName);

                    // Делаем еще одну проверку. Лучше перебдеть. Если все ок, заходим в тело условия и удаляем изображения
                    if (newsImage != null)
                    {
                        // Исходные (полноразмерные) изображения
                        FileInfo imageNormal = new FileInfo(_appEnvironment.WebRootPath + newsImage.ImagePathOriginal);
                        if (imageNormal.Exists)
                        {
                            imageNormal.Delete();
                        }
                        // И их уменьшенные копии
                        FileInfo imageScaled = new FileInfo(_appEnvironment.WebRootPath + newsImage.ImagePathScaled);
                        if (imageScaled.Exists)
                        {
                            imageScaled.Delete();
                        }
                        // Удаляем информацию об изображениях из БД и сохраняем
                        _websiteDB.NewsImages.Remove(newsImage);
                        await _websiteDB.SaveChangesAsync();
                    }
                }

                // Создаем экземпляр класса News и присваиваем ему значения из БД
                //News news = await _websiteDB.News.FirstAsync(n => n.Id == newsId);

                // Создаем список изображений из БД, закрепленных за выбранной новостью
                List<NewsImage> images = await _websiteDB.NewsImages.Where(i => i.NewsId == newsId).OrderByDescending(i => i.ImageDate).ToListAsync();

                // Создаем модель для передачи в представление и присваиваем значения
                EditNewsViewModel model = new EditNewsViewModel()
                {
                    NewsTitle = news.NewsTitle,
                    NewsBody = news.NewsBody.SpecSymbolsToEdit(),
                    NewsImages = images,
                    // Скрытые поля
                    NewsId = newsId,
                    NewsDate = news.NewsDate,
                    NewsUserName = news.NewsUserName,
                    NewsImagesCount = images.Count
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

        #region Редактировать новость [POST]
        [HttpPost]
        public async Task<IActionResult> EditNews(EditNewsViewModel model, IFormFileCollection uploads)
        {
            int imageSize = 1048576 * 2;

            // Ищем запись в БД по Id
            News news = await _websiteDB.News.FirstOrDefaultAsync(n => n.Id == model.NewsId);

            // Если запись существует, продолжаем обработку
            if (news != null)
            {
                // Проверяем, чтобы общее количество изображений не превышало установленный лимит
                if (uploads.Count > 3 - model.NewsImagesCount)
                {
                    ModelState.AddModelError("NewsImage", $"Превышен лимит изображений для одной записи: 3");
                }
                else
                {
                    // Проверяем, чтобы размер файлов не превышал заданный объем
                    foreach (var file in uploads)
                    {
                        if (file.Length > imageSize)
                        {
                            ModelState.AddModelError("NewsImage", $"Файл \"{file.FileName}\" превышает установленный лимит 2MB.");
                            break;
                        }
                    }
                }

                // Если все в порядке, заходим в тело условия
                if (ModelState.IsValid)
                {
                    //News news = await _websiteDB.News.FirstAsync(n => n.Id == model.NewsId);

                    // Обновляем данные
                    news.NewsTitle = model.NewsTitle;
                    news.NewsBody = model.NewsBody.SpecSymbolsToView();

                    // Далее начинаем обработку загружаемых изображений
                    List<NewsImage> newsImages = new List<NewsImage>();
                    foreach (var uploadedImage in uploads)
                    {
                        // Если размер входного файла больше 0, заходим в тело условия
                        if (uploadedImage.Length > 0)
                        {
                            // Создаем новый объект класса FileInfo из полученного изображения для дальнейшей обработки
                            FileInfo imgFile = new FileInfo(uploadedImage.FileName);
                            // Приводим расширение к нижнему регистру (если оно было в верхнем)
                            string imgExtension = imgFile.Extension.ToLower();
                            // Генерируем новое имя для файла
                            string newFileName = Guid.NewGuid() + imgExtension;
                            // Пути сохранения файла
                            string originalDirectory = "/uploadedfiles/news/images/original/";
                            string scaledDirectory = "/uploadedfiles/news/images/scaled/";
                            string pathOriginal = originalDirectory + newFileName; // изображение исходного размера
                            string pathScaled = scaledDirectory + newFileName; // уменьшенное изображение

                            // Если такие директории не созданы, то создаем их
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
                                    await clone.SaveAsync(_appEnvironment.WebRootPath + pathScaled, new JpegEncoder { Quality = 50 });
                                    // Сохраняем исходное изображение
                                    await image.SaveAsync(_appEnvironment.WebRootPath + pathOriginal);
                                }
                            }
                            // Если вдруг что-то пошло не так (например, на вход подало не картинку), то выводим сообщение об ошибке
                            catch
                            {
                                // Создаем сообщение об ошибке для вывода пользователю
                                ModelState.AddModelError("NewsImage", $"Ошибка при обработке файла {uploadedImage.FileName}. Обратитесь к администратору сайта.");

                                // Удаляем только что созданные файлы (если ошибка возникла не на первом файле)
                                foreach (var image in newsImages)
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

                            // Создаем объект класса NewsImage со всеми параметрами
                            NewsImage newsImage = new NewsImage()
                            {
                                Id = Guid.NewGuid(),
                                ImageName = newFileName,
                                ImagePathOriginal = pathOriginal,
                                ImagePathScaled = pathScaled,
                                NewsId = news.Id,
                                ImageDate = DateTime.Now
                            };
                            // Добавляем объект newsImage в список newsImages
                            newsImages.Add(newsImage);
                        }
                    }

                    // Если в процессе выполнения не возникло ошибок, сохраняем всё в БД
                    if (newsImages != null && newsImages.Count > 0)
                    {
                        await _websiteDB.NewsImages.AddRangeAsync(newsImages);
                    }
                    _websiteDB.News.Update(news);
                    await _websiteDB.SaveChangesAsync();

                    // Редирект на главную страницу
                    return RedirectToAction("Index", "News");
                }

                // В случае, если при редактировании пытаться загрузить картинку выше разрешенного лимита, то перестают отображаться уже имеющиеся изображения
                // При перегонке модели из гет в пост, теряется список с изображениями. Причина пока не ясна, поэтому сделал такой костыль
                // Счетчик соответственно тоже обнулялся, поэтому его тоже приходится переназначать заново
                List<NewsImage> images = await _websiteDB.NewsImages.Where(i => i.NewsId == model.NewsId).OrderByDescending(i => i.ImageDate).ToListAsync();
                model.NewsImages = images;
                model.NewsImagesCount = images.Count;

                // Возврат модели в представление в случае, если запорится валидация
                return View(model);
            }
            else
            {
                return Redirect("/Main/PageNotFound");
            }
        }
        #endregion

        #region Удалить новость [POST]
        public async Task<IActionResult> DeleteNews(Guid newsId)
        {
            // Находим запись в БД по Id
            News news = await _websiteDB.News.FirstOrDefaultAsync(n => n.Id == newsId);

            // Проверяем, чтобы такая запись существовала
            if (news != null)
            {
                // Создаем список из привязанных к удаляемой записи изображений
                List<NewsImage> newsImages = await _websiteDB.NewsImages.Where(i => i.NewsId == newsId).ToListAsync();

                if (newsImages.Count > 0)
                {
                    // Удаление изображений из папок
                    foreach (var image in newsImages)
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

                    // Удаляем изображения из БД
                    // Эта строка не обязательна, т.к. при удалении новости, записи с изображениями теряют связь по Id и трутся сами
                    // Достаточно просто удалить изображения из папок, что уже сделано выше
                    _websiteDB.NewsImages.RemoveRange(newsImages);
                }

                // Удаляем новость из БД
                _websiteDB.News.Remove(news);
                await _websiteDB.SaveChangesAsync();

                return RedirectToAction("Index", "News");
            }
            else
            {
                return Redirect("/Main/PageNotFound");
            }
        }
        #endregion

    }
}
