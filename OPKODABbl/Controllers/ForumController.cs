using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Account;
using OPKODABbl.Models.Forum;
using OPKODABbl.Service;
using OPKODABbl.ViewModels.Forum;

namespace OPKODABbl.Controllers
{
    public class ForumController : Controller
    {
        private readonly WebsiteContext _websiteDB;

        public ForumController(WebsiteContext websiteContext)
        {
            _websiteDB = websiteContext;
        }

        #region Главная страница форума
        public async Task<IActionResult> Index()
        {
            // Уровень доступа пользователя по умолчанию
            int userAccessLevel = 1;

            // Если пользователь аутентифицирован, у него может оказаться более высокий уровень доступа
            if (User.Identity.IsAuthenticated)
            {
                User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
                if (user != null)
                {
                    // Устанавливаем уровень доступа для авторизированного пользователя
                    userAccessLevel = user.Role.AccessLevel;
                }
            }

            // Формируем список разделов в соответствии с уровнем доступа юзера, включая подраздела, последние темы и сообщения в нём
            List<Section> sections = await _websiteDB.Sections.Where(s => s.SectionAccessLevel <= userAccessLevel)
                                                              .Include(s => s.Subsections).ThenInclude(s => s.Topics).ThenInclude(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.CharacterClass)
                                                              .OrderBy(s => s.SectionPosition).ToListAsync();

            // Придется прибегнуть к такому говнокоду для разгрузки страницы Index от кода рпи формировании
            foreach (var section in sections)
            {
                foreach (var subsection in section.Subsections)
                {
                    foreach (var topic in subsection.Topics)
                    {
                        // Упорядочиваем все ответы в теме по дате
                        topic.Replies = topic.Replies.OrderBy(r => r.ReplyDate).ToList();
                    }

                    // Упорядочиваем все темы в разделе по дате последнего ответа (ответы уже упорядочили выше)
                    subsection.Topics = subsection.Topics.OrderBy(t => t.Replies.Last().ReplyDate).ToList();
                }
            }

            // Передаем в представление
            return View(sections);
        }
        #endregion

        #region Просмотр подраздела форума
        public async Task<IActionResult> Subsection(Guid subsectionId, int page = 1)
        {
            // Уровень доступа пользователя по умолчанию
            int userAccessLevel = 1;

            // Если пользователь аутентифицирован, у него может оказаться более высокий уровень доступа
            if (User.Identity.IsAuthenticated)
            {
                User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
                if (user != null)
                {
                    // Устанавливаем уровень доступа для авторизированного пользователя
                    userAccessLevel = user.Role.AccessLevel;
                }
            }

            // Формируем модель подраздела с проверкой уровня доступа, а так же со всеми темами, находящимися в подразделе и пр.
            Subsection subsection = await _websiteDB.Subsections.Where(s => s.Section.SectionAccessLevel <= userAccessLevel)
                                                                .Include(s => s.Topics).ThenInclude(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.CharacterClass)
                                                                .FirstOrDefaultAsync(s => s.Id == subsectionId);

            // Проверяем существование подраздела в общем и для данного пользователя в частности (при недостатке прав он будет равен null)
            if (subsection != null)
            {
                // Сортировка ответов в каждой теме по дате
                subsection.Topics.ForEach(t => t.Replies = t.Replies.OrderBy(r => r.ReplyDate).ToList());
                // Сортировка тем в подразделе по дате последнего ответа и разбив по страницам
                int count = subsection.Topics.Count();
                int pageSize = 10;
                subsection.Topics = subsection.Topics.OrderByDescending(t => t.Replies.Last().ReplyDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
                ViewBag.CurrentPage = page;

                // Передаем в представление
                return View(subsection);
            }

            // Если такого раздела не существует или у пользователя нет прав на просмотр подраздела, отправляем ему ошибку 404
            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Просмотр топика
        public async Task<IActionResult> ViewTopic(Guid topicId, int page = 1)
        {
            // Уровень доступа пользователя по умолчанию
            int userAccessLevel = 1;

            // Если пользователь аутентифицирован, у него может оказаться более высокий уровень доступа
            if (User.Identity.IsAuthenticated)
            {
                User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
                if (user != null)
                {
                    // Устанавливаем уровень доступа для авторизированного пользователя
                    userAccessLevel = user.Role.AccessLevel;
                }
            }

            // Если такой топик существует, выполняем дальше
            if (await _websiteDB.Topics.FirstOrDefaultAsync(t => t.Id == topicId) != null)
            {
                // Формируем модель топика со всеми ответами и пользователями в нём. И обязательно проверка на уровень доступа пользователя, для избежания неавторизованного просмотра по прямой ссылке
                Topic topic = await _websiteDB.Topics.Where(t => t.Subsection.Section.SectionAccessLevel <= userAccessLevel)
                                                     .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.AvatarImage)
                                                     .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.Role)
                                                     .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.CharacterClass)
                                                     .Include(t => t.Subsection)
                                                     .FirstOrDefaultAsync(t => t.Id == topicId);

                // Разбиваем ответы по страницам
                int count = topic.Replies.Count();
                int pageSize = 10;
                topic.Replies = topic.Replies.OrderBy(r => r.ReplyDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Замена символов и ББ-код в каждом ответе
                topic.Replies.ForEach(r => r.ReplyBody = r.ReplyBody.Replace(r.ReplyBody, r.ReplyBody.SpecSymbolsToView().BbCode()));

                ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
                ViewBag.CurrentPage = page;

                return View(topic);
            }

            // Если топик не существует, выводим сообщение об ошибке
            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Создать топик[GET]
        [HttpGet]
        public async Task<IActionResult> CreateTopic(Guid subsectionId)
        {
            // Проверка, чтобы пользователь был авторизован
            if (User.Identity.IsAuthenticated)
            {
                // Формируем модель подраздела
                Subsection subsection = await _websiteDB.Subsections.Include(s => s.Section).FirstOrDefaultAsync(s => s.Id == subsectionId);

                // Проверяем, что такой подраздел существует
                if (subsection != null)
                {
                    ViewBag.SubsectionName = subsection.SubsectionName;

                    // Формируем модель пользователя с уровнем доступа для проверки
                    User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);

                    // Проверяем, может ли пользователь просматривать данный подраздел
                    if (subsection.Section.SectionAccessLevel <= user.Role.AccessLevel)
                    {
                        // Если может, то возвращаем ему представление
                        ViewBag.SubsectionId = subsection.Id;
                        return View();
                    }
                }

                // А если не может, то возвращаем ему ошибку 404
                return Redirect("/Main/PageNotFound");
            }

            // Если юзер не авторизован, перенаправляем его на страницу авторизации
            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Создать топик [POST]
        public async Task<IActionResult> CreateTopic(CreateTopicViewModel model)
        {
            // Проверяем, чтобы пользователь был авторизован
            if (User.Identity.IsAuthenticated)
            {
                // Проверка валидации модели
                if (ModelState.IsValid)
                {
                    // Формируем модель подраздела
                    Subsection subsection = await _websiteDB.Subsections.Include(s => s.Section).FirstOrDefaultAsync(s => s.Id == model.SubsectionId);
                    // Проверяем, чтобы такой существовал
                    if (subsection != null)
                    {
                        // Формируем омдель авторизованного пользователя с его ролью
                        User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
                        // Проверяем, чтобы уровень доступа пользователя был не ниже уровня раздела
                        if (user != null && subsection.Section.SectionAccessLevel <= user.Role.AccessLevel)
                        {
                            // Создаем модель топика
                            Topic topic = new Topic()
                            {
                                Id = Guid.NewGuid(),
                                Subsection = subsection,
                                TopicName = model.TopicName,
                                TopicDate = DateTime.Now,
                            };

                            // И модель первого в нём сообщения, которое и будет сообщением только что созданной темы
                            Reply reply = new Reply()
                            {
                                Id = Guid.NewGuid(),
                                ReplyBody = model.TopicBody,
                                ReplyDate = DateTime.Now,
                                User = user,
                                Topic = topic
                            };

                            // Добавляем всё в базу и сохраняем
                            await _websiteDB.Topics.AddAsync(topic);
                            await _websiteDB.Replies.AddAsync(reply);
                            await _websiteDB.SaveChangesAsync();

                            // Редирект в только что созданную тему
                            return RedirectToAction("ViewTopic", "Forum", new { topicId = topic.Id });
                        }
                    }

                    // Ошибка 404, если подраздел не найден
                    return Redirect("/Main/PageNotFound");
                }

                // Это должен быть возврат модели с ошибками. Пока не закончено!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                return View(model);
            }

            // Если пользователь не авторизован, перенаправляем его на страницу авторизации
            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Ответить [GET]
        [HttpGet]
        public async Task<IActionResult> AddReply(Guid topicId, Guid? quoteMessageId)
        {
            ViewBag.TopicId = topicId;
            Topic topic = await _websiteDB.Topics.Include(t => t.Subsection).FirstOrDefaultAsync(t => t.Id == topicId);
            ViewBag.Topic = topic;

            // Цитирование
            if (quoteMessageId != null)
            {
                Reply reply = await _websiteDB.Replies.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == quoteMessageId);

                if (reply != null)
                {
                    AddReplyViewModel model = new AddReplyViewModel()
                    {
                        TopicId = topicId,
                        ReplyBody = $"{reply.User.Name} писал(а):<br>".SpecSymbolsToEdit() + "[quote]" + reply.ReplyBody + "[/quote]" + "<br>".SpecSymbolsToEdit()
                    };

                    return View(model);
                }
            }

            return View();
        }
        #endregion

        #region Ответить [POST]
        [HttpPost]
        public async Task<IActionResult> AddReply(AddReplyViewModel model)
        {
            // Проверяем, чтобы пользователь был авторизован
            if (User.Identity.IsAuthenticated)
            {
                // Проверка валидации модели
                if (ModelState.IsValid)
                {
                    // Формируем модель топика
                    Topic topic = await _websiteDB.Topics.Include(t => t.Replies).FirstOrDefaultAsync(t => t.Id == model.TopicId);

                    // Проверяем, чтобы такой существовал
                    if (topic != null)
                    {
                        // Формируем модель пользователя
                        User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == User.Identity.Name);

                        // Проверяем, что такой действительно существует
                        if (user != null)
                        {
                            // Создаем модель ответа
                            Reply reply = new Reply()
                            {
                                Id = Guid.NewGuid(),
                                ReplyBody = model.ReplyBody,
                                Topic = topic,
                                User = user,
                                ReplyDate = DateTime.Now
                            };

                            // Добавляем в базу и сохраняем
                            await _websiteDB.Replies.AddAsync(reply);
                            await _websiteDB.SaveChangesAsync();


                            // Формирование ссылки на последнее сообщение в теме (которое должно быть нашим сообщением)
                            string lastMessageLink = $"/Forum/ViewTopic?topicId={topic.Id}&page={(int)Math.Ceiling(topic.Replies.Count() / (double)10)}#{topic.Replies.Count()}";


                            // Редирект на тему
                            return Redirect(lastMessageLink); //RedirectToAction("ViewTopic", "Forum", new { topicId = topic.Id });
                        }
                    }

                    // Если топик или пользователь не найдены, выдаём ошибку 404
                    return Redirect("/Main/PageNotFound");
                }

                return View(model);
            }

            // Если пользователь не авторизован, перенаправляем его на страницу авторизации
            return RedirectToAction("Login", "Account");
        }
        #endregion

    }
}
