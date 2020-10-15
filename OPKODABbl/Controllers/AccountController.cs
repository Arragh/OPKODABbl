using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Account;
using OPKODABbl.Service;
using OPKODABbl.ViewModels.Account;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OPKODABbl.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebsiteContext _websiteDB;
        private IWebHostEnvironment _appEnvironment;

        public AccountController(WebsiteContext websiteDbContext, IWebHostEnvironment appEnvironment)
        {
            _websiteDB = websiteDbContext;
            _appEnvironment = appEnvironment;
        }

        #region Регистрация [GET]
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Title = "Регистрация нового пользователя";

            SelectList ingameClasses = new SelectList(_websiteDB.CharacterClasses, "Id", "ClassName");
            ViewBag.Classes = ingameClasses;

            return View();
        }
        #endregion

        #region Регистрация [POST]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Title = "Регистрация нового пользователя";

            // Проверка имени пользователя на наличие запрещенных символов
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
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
            }

            // Проверяем пользователя на дубликат по имени
            User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == model.Name);
            if (user != null)
            {
                ModelState.AddModelError("Name", "Пользователь с таким именем уже существует.");
            }
            // И по адресу Email
            user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                ModelState.AddModelError("Email", "Данный адрес Email уже занят.");
            }

            // Если все в порядке, то заходим в тело условия
            if (ModelState.IsValid)
            {
                // добавляем пользователя в бд
                Role role = await _websiteDB.Roles.FirstOrDefaultAsync(r => r.AccessLevel == 1);

                User newUser = new User()
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password.HashString(),
                    RegisterDate = DateTime.Now,
                    IsConfirmed = false,
                    ConfirmationKey = Guid.NewGuid().ToString(),
                    CharacterClassId = model.CharacterClassId,
                    Role = role
                };

                // Создаем аватар по дефолту
                AvatarImage avatar = new AvatarImage()
                {
                    Id = Guid.NewGuid(),
                    ImageName = "default",
                    ImagePath = "/images/avatar_default.jpg",
                    ImageDate = DateTime.Now,
                    UserId = newUser.Id
                };

                _websiteDB.Users.Add(newUser);
                _websiteDB.AvatarImages.Add(avatar);
                await _websiteDB.SaveChangesAsync();

                // Отправка письма для подтверждения регистрации на Email
                await SendEmailConfirmation(newUser.Id, newUser.Email, newUser.ConfirmationKey);

                //return RedirectToAction("Index", "Main");
                return RedirectToAction("ConfirmationStatus", "Account", new { message = "Письмо с подтверждением регистрации было отправлено на указанный адрес Email." });
            }

            // В случае ошибок валидации возвращаем модель с сообщениями об ошибках
            SelectList ingameClasses = new SelectList(_websiteDB.CharacterClasses, "Id", "ClassName");
            ViewBag.Classes = ingameClasses;

            return View(model);
        }
        #endregion

        #region Авторизация [GET]
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Title = "Авторизация";

            return View();
        }
        #endregion

        #region Авторизация [POST]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ViewBag.Title = "Авторизация";

            if (ModelState.IsValid)
            {
                User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == model.Name && u.Password == model.Password.HashString());

                if (user != null)
                {
                    if (!user.IsConfirmed)
                    {
                        ModelState.AddModelError("Password", "Подтвердите регистрацию по ссылке в письме, отправленной вам на Email.");
                        return View(model);
                    }

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
            User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == userName);

            // Находим его аватар
            AvatarImage avatarImage = await _websiteDB.AvatarImages.FirstOrDefaultAsync(a => a.UserId == user.Id);

            // Проверяем, чтобы пользователь не был NUll и чтобы имя залогиненного пользователя совпадало с полученным в методе
            if (User.Identity.Name == userName && user != null)
            {
                ViewBag.Title = $"Редактирование профиля {user.Name}";

                // Создаем модель для передачи в представление
                EditProfileViewModel model = new EditProfileViewModel()
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CharacterClassId = user.CharacterClassId
                };

                // Аватар
                if (avatarImage != null)
                {
                    model.AvatarImage = avatarImage.ImagePath;
                }

                SelectList ingameClasses = new SelectList(_websiteDB.CharacterClasses, "Id", "ClassName", user.CharacterClassId);
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

            SelectList ingameClasses = new SelectList(_websiteDB.CharacterClasses, "Id", "ClassName");
            ViewBag.Classes = ingameClasses;

            // Ищем такого юзера в БД
            User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == model.UserId);

            // Проверяем, что такой юзер существует
            if (user != null)
            {
                ViewBag.Title = $"Редактирование профиля {user.Name}";

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
                        AvatarImage temp = await _websiteDB.AvatarImages.FirstOrDefaultAsync(a => a.UserId == user.Id);

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
                        await _websiteDB.AvatarImages.AddAsync(avatar);
                    }

                    // Обновляем данные пользователя на полученные данные с модели
                    user.Email = model.Email;
                    if (!string.IsNullOrWhiteSpace(model.CharacterClassId.ToString()))
                    {
                        user.CharacterClassId = model.CharacterClassId;
                    }
                    // Если был задан новый пароль, обновляем и его
                    if (!string.IsNullOrWhiteSpace(model.NewPassword))
                    {
                        user.Password = model.NewPassword.HashString();
                    }

                    // Сохраняем
                    _websiteDB.Users.Update(user);
                    await _websiteDB.SaveChangesAsync();

                    // Перезаписываем куки на новые, с обновленными данными
                    await Authenticate(user);

                    ViewBag.Successful = "Профиль сохранён";

                    // Переназначение аватара, иначе он теряется
                    AvatarImage temp1 = await _websiteDB.AvatarImages.FirstOrDefaultAsync(a => a.UserId == user.Id);
                    if (temp1 != null)
                    {
                        model.AvatarImage = temp1.ImagePath;
                    }
                    // Переназначение имени пользователя
                    model.Name = user.Name;

                    return View(model);
                    //return RedirectToAction("EditProfile", "Account", new { userName = user.Name });
                }

                // Переназначение аватара в случае ошибки валидации, иначе они теряется
                AvatarImage temp2 = await _websiteDB.AvatarImages.FirstOrDefaultAsync(a => a.UserId == user.Id);
                if (temp2 != null)
                {
                    model.AvatarImage = temp2.ImagePath;
                }

                model.Name = user.Name;

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

        #region Повторная отправка письма с подтверждением [GET]
        [HttpGet]
        public IActionResult AccountConfirmation()
        {
            ViewBag.Title = "Подтверждение регистрации";

            return View();
        }
        #endregion

        #region Повторная отправка письма с подтверждением [POST]
        [HttpPost]
        public async Task<IActionResult> AccountConfirmation(AccountConfirmationViewModel model)
        {
            ViewBag.Title = "Подтверждение регистрации";

            User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user != null)
            {
                if (user.IsConfirmed)
                {
                    ModelState.AddModelError("Email", "Пользователь уже подтвердил регистрацию.");
                    return View(model);
                }

                // Отправка письма для подтверждения регистрации на Email
                await SendEmailConfirmation(user.Id, user.Email, user.ConfirmationKey);

                return RedirectToAction("ConfirmationStatus", "Account", new { message = "Письмо с подтверждением регистрации было отправлено на указанный адрес Email." });
            }

            ModelState.AddModelError("Email", "Пользователь не найден.");
            return View(model);
        }
        #endregion

        #region Восстановление пароля [GET]
        [HttpGet]
        public IActionResult ResetPassword()
        {
            ViewBag.Title = "Восстановление пароля";

            return View();
        }
        #endregion

        #region Восстановление пароля [POST]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            ViewBag.Title = "Восстановление пароля";

            if (ModelState.IsValid)
            {
                User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == model.Name && u.Email == model.Email);
                if (user != null)
                {
                    string allowedCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                    Random rnd = new Random();

                    string newPassword = "";

                    for (int i = 0; i < 6; i++)
                    {
                        newPassword += allowedCharacters[rnd.Next(allowedCharacters.Length)];
                    }

                    user.Password = newPassword.HashString();

                    _websiteDB.Users.Update(user);
                    await _websiteDB.SaveChangesAsync();

                    await SendEmailNewPassword(user.Name, user.Email, newPassword);

                    return RedirectToAction("ConfirmationStatus", "Account", new { message = "Новый пароль отправлен на почту." });
                }

                return RedirectToAction("ConfirmationStatus", "Account", new { message = "Пользователь с таким сочетанием Логина и Email не найден." });
            }

            ModelState.AddModelError("Email", "Что-то пошло не так...");

            return View(model);
        }
        #endregion

        #region Отправка письма с подтверждением регистрации
        public async Task SendEmailConfirmation(Guid userId, string userEmail, string confirmationKey)
        {
            string mailLink = "https://" + "localhost:44358/Account/EmailConfirmation?" + $"userId={userId}&" + $"confirmationKey={confirmationKey}";
            string subject = "Подтверждение регистрации на сайте оркодав.ру";
            string message = $"Для подтверждения регистрации пройдите по ссылке, скопировав её в адресную строку браузера. Если вы не регистрировались, то просто удалите это письмо и забудьте.<br> <a href=\"{mailLink}\">Подтвердить регистрацию</a>";

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта orcodav.ru", "alexvolkov-777@mail.ru"));
            emailMessage.To.Add(new MailboxAddress("", userEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.mail.ru", 25, false);
                await client.AuthenticateAsync("alexvolkov-777@mail.ru", "Ytrewq#21");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
        #endregion

        #region Отправка письма с новым паролем
        public async Task SendEmailNewPassword(string userName, string userEmail, string newPassword)
        {
            string subject = "Новый пароль на сайте оркодав.ру";
            string message = $"Новый пароль на сайте orcodav.ru<br>Логин: {userName}<br>Пароль: {newPassword}";

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта orcodav.ru", "alexvolkov-777@mail.ru"));
            emailMessage.To.Add(new MailboxAddress("", userEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.mail.ru", 25, false);
                await client.AuthenticateAsync("alexvolkov-777@mail.ru", "Ytrewq#21");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
        #endregion

        #region Метод подтверждения регистрации
        [HttpGet]
        public async Task<IActionResult> EmailConfirmation(Guid userId, string confirmationKey)
        {
            User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Id == userId && u.ConfirmationKey == confirmationKey);
            if (user != null)
            {
                if (user.IsConfirmed)
                {
                    return RedirectToAction("ConfirmationStatus", "Account", new { message = "Пользователь уже подтвердил регистрацию." });
                }

                user.IsConfirmed = true;

                _websiteDB.Users.Update(user);
                await _websiteDB.SaveChangesAsync();

                //return RedirectToAction("Index", "Main");
                return RedirectToAction("ConfirmationStatus", "Account", new { message = "Регистрация подтверждена." });
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region ConfirmationStatus
        public IActionResult ConfirmationStatus(string message)
        {
            ViewBag.Title = "Статус регистрации";
            ViewBag.Message = message;

            return View();
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
