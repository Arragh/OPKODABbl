using System;
using System.Collections.Generic;
using System.Linq;
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
                // Берем настройки из БД
                SettingsForum settings = await _websiteDB.SettingsForum.FirstAsync();

                // Сортировка ответов в каждой теме по дате
                subsection.Topics.ForEach(t => t.Replies = t.Replies.OrderBy(r => r.ReplyDate).ToList());
                // Сортировка тем в подразделе по дате последнего ответа и разбив по страницам
                int count = subsection.Topics.Count();
                int subsectionPageSize = settings.SubsectionPageSize;
                int topicPageSize = settings.TopicPageSize;

                subsection.Topics = subsection.Topics.OrderByDescending(t => t.Announcement).ThenByDescending(t => t.Replies.Last().ReplyDate).Skip((page - 1) * subsectionPageSize).Take(subsectionPageSize).ToList();

                ViewBag.TotalPages = (int)Math.Ceiling(count / (double)subsectionPageSize);
                ViewBag.CurrentPage = page;
                ViewBag.TopicPageSize = topicPageSize;

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

            // Формируем модель топика со всеми ответами и пользователями в нём. И обязательно проверка на уровень доступа пользователя, для избежания неавторизованного просмотра по прямой ссылке
            Topic topic = await _websiteDB.Topics.Where(t => t.Subsection.Section.SectionAccessLevel <= userAccessLevel)
                                                 .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.AvatarImage)
                                                 .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.Role)
                                                 .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.CharacterClass)
                                                 .Include(t => t.Subsection)
                                                 .FirstOrDefaultAsync(t => t.Id == topicId);

            // Если у юзера недостаточно прав для просмотра, то топик будет null
            if (topic != null)
            {
                // Берем настройки из БД
                SettingsForum settings = await _websiteDB.SettingsForum.FirstAsync();

                // Разбиваем ответы по страницам
                int count = topic.Replies.Count();
                int pageSize = settings.TopicPageSize;

                topic.Replies = topic.Replies.OrderBy(r => r.ReplyDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
                // Замена символов и ББ-код в каждом ответе
                topic.Replies.ForEach(r => r.ReplyBody = r.ReplyBody.Replace(r.ReplyBody, r.ReplyBody.SpecSymbolsToView().BbCode()));

                ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
                ViewBag.CurrentPage = page;

                return View(topic);
            }

            // Если топик не существует для пользователя, выводим сообщение об ошибке
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
                    ViewBag.SubsectionId = subsection.Id;
                    ViewBag.SubsectionName = subsection.SubsectionName;

                    // Формируем модель пользователя с уровнем доступа для проверки
                    User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);

                    // Проверяем, может ли пользователь просматривать данный подраздел
                    if (subsection.Section.SectionAccessLevel <= user.Role.AccessLevel)
                    {
                        // Если может, то возвращаем ему представление
                        //ViewBag.SubsectionId = subsection.Id;
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
            // Формируем модель подраздела
            Subsection subsection = await _websiteDB.Subsections.Include(s => s.Section).FirstOrDefaultAsync(s => s.Id == model.SubsectionId);
            ViewBag.SubsectionId = subsection.Id;
            ViewBag.SubsectionName = subsection.SubsectionName;

            // Проверяем, чтобы пользователь был авторизован
            if (User.Identity.IsAuthenticated)
            {
                // Проверка валидации модели
                if (ModelState.IsValid)
                {
                    
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
                                Announcement = model.Announcement
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
                // Эти 2 вьюбага необходимы для работы колбасы ссылок навигации на странице
                ViewBag.Topic = await _websiteDB.Topics.Include(t => t.Subsection).FirstOrDefaultAsync(t => t.Id == model.TopicId);
                ViewBag.TopicId = model.TopicId;

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

        #region Редактировать [GET]
        [HttpGet]
        public async Task<IActionResult> EditReply(Guid topicId, Guid replyId, int htmlAnchor)
        {
            // Эти 2 вьюбага необходимы для работы колбасы ссылок навигации на странице
            ViewBag.Topic = await _websiteDB.Topics.Include(t => t.Subsection).FirstOrDefaultAsync(t => t.Id == topicId);
            ViewBag.TopicId = topicId;

            if (User.Identity.IsAuthenticated)
            {
                // Ищем сообщение по нескольким параметрам
                Reply reply = await _websiteDB.Replies.Include(r => r.Topic).FirstOrDefaultAsync(r => r.Id == replyId && r.Topic.Id == topicId && r.User.Name == User.Identity.Name);

                // Если сообщение с заданными параметрами найдено, то заходим в условие
                if (reply != null && Convert.ToInt32((DateTime.Now - reply.ReplyDate).TotalMinutes) < 10)
                {
                    // Формируем модель редактирования
                    EditReplyViewModel model = new EditReplyViewModel()
                    {
                        ReplyId = reply.Id,
                        TopicId = reply.Topic.Id,
                        ReplyBody = reply.ReplyBody,
                        HtmlAnchor = htmlAnchor
                    };

                    // Передаем модель в представление
                    return View(model);
                }

                // Если сообщение не найдено, выдаем ошибку 404
                return Redirect("/Main/PageNotFound");
            }

            // Если пользователь не авторизован, перенаправляем его на страницу авторизации
            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Редактировать [POST]
        [HttpPost]
        public async Task<IActionResult> EditReply(EditReplyViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Эти 2 вьюбага необходимы для работы колбасы ссылок навигации на странице
                ViewBag.Topic = await _websiteDB.Topics.Include(t => t.Subsection).FirstOrDefaultAsync(t => t.Id == model.TopicId);
                ViewBag.TopicId = model.TopicId;

                if (ModelState.IsValid)
                {
                    Reply reply = await _websiteDB.Replies.Include(r => r.Topic).ThenInclude(t => t.Replies).FirstOrDefaultAsync(r => r.Id == model.ReplyId && r.Topic.Id == model.TopicId && r.User.Name == User.Identity.Name);

                    // Проверяем есть ли такое сообщение и не истекло ли время на его редактирование (тут делаем время + 10 минут, дав пользователю фору на сам процесс редактирования)
                    if (reply != null && Convert.ToInt32((DateTime.Now - reply.ReplyDate).TotalMinutes) < 20)
                    {
                        // Изменяем тело сообщения
                        reply.ReplyBody = model.ReplyBody;

                        // СОхраняем изменения
                        _websiteDB.Replies.Update(reply);
                        await _websiteDB.SaveChangesAsync();

                        // Формируем прямую ссылку на сообщение
                        string EditedMessageLink = $"/Forum/ViewTopic?topicId={model.TopicId}&page={(int)Math.Ceiling(reply.Topic.Replies.Count() / (double)10)}#{model.HtmlAnchor}";

                        // Редирект по прямой ссылке
                        return Redirect(EditedMessageLink);
                    }

                    // Если сообщение не найдено, выдаём ошибку 404
                    return Redirect("/Main/PageNotFound");
                }

                // Если модель не прошла валидацию, возвращаем её в представление
                return View(model);
            }

            // Если пользователь не авторизован, перенаправляем его на страницу авторизации
            return RedirectToAction("Login", "Account");
        }
        #endregion

    }
}
