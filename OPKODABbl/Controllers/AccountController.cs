using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Account;
using OPKODABbl.Service;
using OPKODABbl.ViewModels.Account;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace OPKODABbl.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsersContext _usersDB;
        private IWebHostEnvironment _appEnvironment;

        public AccountController(UsersContext usersDbContext, IWebHostEnvironment appEnvironment)
        {
            _usersDB = usersDbContext;
            _appEnvironment = appEnvironment;
        }

        #region Регистрация [GET]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        #endregion

        #region Регистрация [POST]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Проверка имени пользователя на наличие запрещенных символов
            string allowedUserNameCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя";
            foreach (var inputChar in model.Name)
            {
                bool errorTrigger = false;

                foreach (var allowedChar in allowedUserNameCharacters)
                {
                    if (inputChar == allowedChar)
                    {
                        errorTrigger = true;
                        break;
                    }
                }
                if (!errorTrigger)
                {
                    ModelState.AddModelError("Name", "Имя содержит запрещенные символы");
                    break;
                }
            }

            // Проверяем пользователя на дубликат по имени
            User user = await _usersDB.Users.FirstOrDefaultAsync(u => u.Name == model.Name);
            if (user != null)
            {
                ModelState.AddModelError("Name", "Пользователь с таким именем уже существует.");
            }
            // И по адресу Email
            user = await _usersDB.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                ModelState.AddModelError("Email", "Данный адрес Email уже занят.");
            }

            // Если все в порядке, то заходим в тело условия
            if (ModelState.IsValid)
            {
                // добавляем пользователя в бд
                Role role = await _usersDB.Roles.FirstOrDefaultAsync(r => r.Name == "user");

                User newUser = new User { Id = Guid.NewGuid(), Name = model.Name, Email = model.Email, Password = model.Password.HashString(), Role = role };

                // Создаем аватар по дефолту
                AvatarImage avatar = new AvatarImage()
                {
                    Id = Guid.NewGuid(),
                    ImageName = "default",
                    ImagePath = "/images/avatar_default.jpg",
                    ImageDate = DateTime.Now,
                    UserId = newUser.Id
                };

                _usersDB.Users.Add(newUser);
                _usersDB.AvatarImages.Add(avatar);
                await _usersDB.SaveChangesAsync();

                await Authenticate(newUser); // аутентификация

                return RedirectToAction("Index", "Main");
            }

            // В случае ошибок валидации возвращаем модель с сообщениями об ошибках
            return View(model);
        }
        #endregion

        #region Авторизация [GET]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        #endregion

        #region Авторизация [POST]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _usersDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == model.Name && u.Password == model.Password.HashString());
                if (user != null)
                {
                    await Authenticate(user); // аутентификация

                    if (model.ReturnUrl != null)
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Main");
                }
                ModelState.AddModelError("Password", "Неверное сочетание логина и пароля");
            }
            return View(model);
        }
        #endregion

        #region Редактирование профиля [GET]
        [HttpGet]
        public async Task<IActionResult> EditProfile(string userName)
        {
            // Находим пользователя по имени
            User user = await _usersDB.Users.FirstOrDefaultAsync(u => u.Name == userName);

            // Находим его аватар
            AvatarImage avatarImage = await _usersDB.AvatarImages.FirstOrDefaultAsync(a => a.UserId == user.Id);

            // Проверяем, чтобы пользователь не был NUll и чтобы имя залогиненного пользователя совпадало с полученным в методе
            if (User.Identity.Name == userName && user != null)
            {
                // Создаем модель для передачи в представление
                EditProfileViewModel model = new EditProfileViewModel()
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CharacterName = user.CharacterName,
                    IngameClassId = user.IngameClassId
                };

                // Аватар
                if (avatarImage != null)
                {
                    model.AvatarImage = avatarImage.ImagePath;
                }

                SelectList ingameClasses = new SelectList(_usersDB.CharacterClasses, "Id", "ClassName", user.IngameClassId);
                ViewBag.Classes = ingameClasses;

                return View(model);
            }

            // Возврат ошибки 404, если пользователь не найден или пытается редактировать не свой профиль
            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Редактирование профиля [POST]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model, IFormFile avatarImage)
        {
            int imageSize = 1048576 * 2;

            // Ищем такого юзера в БД
            User user = await _usersDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == model.UserId);

            // Проверяем, что такой юзер существует
            if (user != null)
            {
                // Проверяем подтверждающий пароль
                if (!string.IsNullOrWhiteSpace(model.Password) && model.Password.HashString() != user.Password)
                {
                    ModelState.AddModelError("Password", "Неверный пароль");
                }

                // Проверяем, загружал ли юзер новый аватар
                if (avatarImage != null && avatarImage.Length > imageSize)
                {
                    ModelState.AddModelError("AvatarImage", $"Файл \"{avatarImage.FileName}\" превышает установленный лимит 2MB.");
                }

                // Если все ок, заходим
                if (ModelState.IsValid)
                {
                    // Если был загружен новый аватар, заходим и выполняем
                    if (avatarImage != null)
                    {
                        // Создаем новый объект класса FileInfo из полученного изображения для дальнейшей обработки
                        FileInfo imgFile = new FileInfo(avatarImage.FileName);
                        // Приводим расширение файла к нижнему регистру
                        string imgExtension = imgFile.Extension.ToLower();
                        // Генерируем новое имя для файла
                        string newFileName = Guid.NewGuid() + imgExtension;
                        // Пути сохранения файла
                        string avatarDirectory = "/uploadedfiles/account/images/avatar/";
                        // Если такой директории не существует, то создаем её
                        if (!Directory.Exists(_appEnvironment.WebRootPath + avatarDirectory))
                        {
                            Directory.CreateDirectory(_appEnvironment.WebRootPath + avatarDirectory);
                        }
                        // Путь для аватара
                        string pathAvatarImage = avatarDirectory + newFileName;

                        // В операторе try/catch делаем уменьшенную копию изображения.
                        // Если входным файлом окажется не изображение, нас перекинет в блок CATCH и выведет сообщение об ошибке
                        try
                        {
                            // Создаем объект класса SixLabors.ImageSharp.Image и грузим в него полученное изображение
                            using (Image image = Image.Load(avatarImage.OpenReadStream()))
                            {
                                // Создаем уменьшенную копию и обрезаем её
                                var clone = image.Clone(x => x.Resize(new ResizeOptions
                                {
                                    Mode = ResizeMode.Crop,
                                    Size = new Size(100, 100)
                                }));
                                // Сохраняем уменьшенную копию
                                await clone.SaveAsync(_appEnvironment.WebRootPath + pathAvatarImage, new JpegEncoder { Quality = 70 });
                            }
                        }
                        // Если вдруг что-то пошло не так (например, на вход подало не картинку), то выводим сообщение об ошибке
                        catch
                        {
                            // Создаем сообщение об ошибке для вывода пользователю
                            ModelState.AddModelError("AvatarImage", $"Файл {avatarImage.FileName} имеет неверный формат.");

                            // Возвращаем модель с сообщением об ошибке в представление
                            return View(model);
                        }

                        // Удаляем предыдущий аватар
                        AvatarImage temp = await _usersDB.AvatarImages.FirstOrDefaultAsync(a => a.UserId == user.Id);

                        // Проверяем, чтобы предыдущий аватар не оказался NULL или дефолтным
                        if (temp != null && temp.ImageName != "default")
                        {
                            FileInfo avatarToDelete = new FileInfo(_appEnvironment.WebRootPath + temp.ImagePath);
                            if (avatarToDelete.Exists)
                            {
                                avatarToDelete.Delete();
                            }
                        }

                        // Создаем новую модель аватара
                        AvatarImage avatar = new AvatarImage()
                        {
                            Id = Guid.NewGuid(),
                            ImageName = newFileName,
                            ImagePath = pathAvatarImage,
                            ImageDate = DateTime.Now,
                            UserId = user.Id
                        };

                        // Сохраняем аватар в БД
                        await _usersDB.AvatarImages.AddAsync(avatar);
                    }

                    // Обновляем данные пользователя на полученные данные с модели
                    user.Name = model.Name;
                    user.Email = model.Email;
                    if (!string.IsNullOrWhiteSpace(model.CharacterName))
                    {
                        user.CharacterName = model.CharacterName;
                    }
                    if (!string.IsNullOrWhiteSpace(model.IngameClassId.ToString()))
                    {
                        user.IngameClassId = model.IngameClassId;
                    }
                    // Если был задан новый пароль, обновляем и его
                    if (!string.IsNullOrWhiteSpace(model.NewPassword))
                    {
                        user.Password = model.NewPassword.HashString();
                    }

                    // Сохраняем
                    _usersDB.Users.Update(user);
                    await _usersDB.SaveChangesAsync();

                    // Перезаписываем куки на новые, с обновленными данными
                    await Authenticate(user);

                    ViewBag.Successful = "Изменения сохранены";

                    // Переназначение аватара в случае ошибки валидации, иначе он теряется
                    AvatarImage temp1 = await _usersDB.AvatarImages.FirstOrDefaultAsync(a => a.UserId == user.Id);
                    model.AvatarImage = temp1.ImagePath;

                    SelectList ingameClasses = new SelectList(_usersDB.CharacterClasses, "Id", "ClassName", user.IngameClassId);
                    ViewBag.Classes = ingameClasses;

                    return View(model);
                    //return RedirectToAction("EditProfile", "Account", new { userName = user.Name });
                }

                // Переназначение аватара в случае ошибки валидации, иначе он теряется
                AvatarImage temp2 = await _usersDB.AvatarImages.FirstOrDefaultAsync(a => a.UserId == user.Id);
                model.AvatarImage = temp2.ImagePath;

                // Возврат модели с ошибкой
                return View(model);
            }

            // Возврат ошибки 404, если пользователь не найден
            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region LogOut
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Main");
        }
        #endregion

        #region Метод Authenticate
        private async Task Authenticate(User user)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        #endregion
    }
}
