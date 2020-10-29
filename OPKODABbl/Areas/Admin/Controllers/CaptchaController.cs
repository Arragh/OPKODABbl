using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Areas.Admin.ViewModels.Captcha;
using OPKODABbl.Models.Account;
using OPKODABbl.Service;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace OPKODABbl.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CaptchaController : Controller
    {
        private WebsiteContext _websiteDB;
        private IWebHostEnvironment _appEnvironment;

        public CaptchaController(WebsiteContext websiteDBContext, IWebHostEnvironment appEnvironment)
        {
            _websiteDB = websiteDBContext;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> AllCaptchas()
        {
            List<Captcha> captchas = await _websiteDB.Captchas.ToListAsync();

            return View(captchas);
        }

        #region Добавить капчу[GET]
        [HttpGet]
        public IActionResult AddCaptcha()
        {
            return View();
        }
        #endregion

        #region Добавить капчу[POST]
        [HttpPost]
        public async Task<IActionResult> AddCaptcha(AddCaptchaViewModel model, IFormFile captchaImage)
        {
            int imageSize = 1048576 * 2;

            // Проверяем, чтобы обязательно был указан файл с изображением
            if (captchaImage == null || captchaImage.Length == 0)
            {
                ModelState.AddModelError("CaptchaImage", "Укажите изображение для показа в слайдере.");
            }
            // Проверяем, чтобы входящий файл не превышал установленный максимальный размер
            if (captchaImage != null && captchaImage.Length > imageSize)
            {
                ModelState.AddModelError("CaptchaImage", $"Файл \"{captchaImage.FileName}\" превышает установленный лимит 2MB.");
            }

            if (ModelState.IsValid)
            {
                // Создаем новый объект класса FileInfo из полученного изображения для дальнейшей обработки
                FileInfo imgFile = new FileInfo(captchaImage.FileName);
                // Приводим расширение к нижнему регистру (если оно было в верхнем)
                string imgExtension = imgFile.Extension.ToLower();
                // Генерируем новое имя для файла
                string newFileName = Guid.NewGuid() + imgExtension;
                // Пути сохранения файла
                string originalDirectory = "/uploadedfiles/captcha/images/original/";
                string scaledDirectory = "/uploadedfiles/captcha/images/scaled/";
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
                    using (Image image = Image.Load(captchaImage.OpenReadStream()))
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
                    ModelState.AddModelError("GalleryImage", $"Файл {captchaImage.FileName} имеет неверный формат.");

                    // Возвращаем модель с сообщением об ошибке в представление
                    return View(model);
                }

                Captcha captcha = new Captcha()
                {
                    Id = Guid.NewGuid(),
                    ImagePathScaled = pathScaled,
                    ImagePathOriginal = pathOriginal,
                    Question = model.Question,
                    Answer = model.Answer
                };

                await _websiteDB.Captchas.AddAsync(captcha);
                await _websiteDB.SaveChangesAsync();

                return RedirectToAction("AllCaptchas", "Captcha");
            }

            // Возврат модели при неудачной валидации
            return View(model);
        }
        #endregion

        #region Удалить капчу[POST]
        public async Task<IActionResult> DeleteCaptcha(Guid captchaId)
        {
            Captcha captcha = await _websiteDB.Captchas.FirstOrDefaultAsync(c => c.Id == captchaId);

            if (captcha != null)
            {
                // Удаляем исходные (полноразмерные) изображения
                FileInfo imageOriginal = new FileInfo(_appEnvironment.WebRootPath + captcha.ImagePathOriginal);
                if (imageOriginal.Exists)
                {
                    imageOriginal.Delete();
                }
                // И их уменьшенные копии
                FileInfo imageScaled = new FileInfo(_appEnvironment.WebRootPath + captcha.ImagePathScaled);
                if (imageScaled.Exists)
                {
                    imageScaled.Delete();
                }

                _websiteDB.Remove(captcha);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("AllCaptchas", "Captcha");
        }
        #endregion

    }
}
