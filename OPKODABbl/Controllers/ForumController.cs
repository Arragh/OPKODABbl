﻿using System;
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
            int userAccessLevel = 1;

            if (User.Identity.IsAuthenticated)
            {
                User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
                if (user != null)
                {
                    userAccessLevel = user.Role.AccessLevel;
                }
            }

            List<Section> sections = await _websiteDB.Sections.Where(s => s.SectionAccessLevel <= userAccessLevel).Include(s => s.Subsections).ThenInclude(s => s.Topics).ThenInclude(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.CharacterClass)
                                                              .OrderBy(s => s.SectionPosition).ToListAsync();

            return View(sections);
        }
        #endregion

        #region Просмотр раздела форума
        public async Task<IActionResult> Subsection(Guid subsectionId)
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

            // Проверяем, чтобы такой подраздел существовал
            if (await _websiteDB.Subsections.FirstOrDefaultAsync(s => s.Id == subsectionId) != null)
            {
                // Создаем модель подраздела с проверкой уровня доступа, а так же со всеми темами, находящимися в подразделе и пр.
                Subsection subsection = await _websiteDB.Subsections.Where(s => s.Section.SectionAccessLevel <= userAccessLevel).Include(s => s.Topics).ThenInclude(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.CharacterClass)
                                                                    .FirstOrDefaultAsync(s => s.Id == subsectionId);

                // Сортировка тем в подразделе по дате последнего ответа
                subsection.Topics.ForEach(s => s.Replies.OrderByDescending(r => r.ReplyDate));
                subsection.Topics = subsection.Topics.OrderByDescending(t => t.Replies.First().ReplyDate).ToList();

                // Передаем в представление
                ViewBag.SubsectionId = subsection.Id;
                return View(subsection);
            }

            // Если подраздел не был найден, возвращаем ошибку 404
            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Просмотр топика
        public async Task<IActionResult> ViewTopic(Guid topicId, int page = 1)
        {
            int userAccessLevel = 1;

            if (User.Identity.IsAuthenticated)
            {
                User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
                if (user != null)
                {
                    userAccessLevel = user.Role.AccessLevel;
                }
            }

            Topic topic = await _websiteDB.Topics.Where(t => t.Subsection.Section.SectionAccessLevel <= userAccessLevel)
                                                 .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.AvatarImage)
                                                 .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.Role)
                                                 .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.CharacterClass)
                                                 .Include(t => t.Subsection)
                                                 .FirstOrDefaultAsync(t => t.Id == topicId);

            if (topic != null)
            {
                int count = topic.Replies.Count();
                int pageSize = 10;
                topic.Replies = topic.Replies.OrderBy(r => r.ReplyDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Замена символов в каждом ответе
                topic.Replies.ForEach(r => r.ReplyBody = r.ReplyBody.Replace(r.ReplyBody, r.ReplyBody.SpecSymbolsToView()));

                ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);

                ViewBag.CurrentPage = page;

                return View(topic);
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Создать топик[GET]
        [HttpGet]
        public async Task<IActionResult> CreateTopic(Guid subsectionId)
        {
            if (User.Identity.IsAuthenticated)
            {
                Subsection subsection = await _websiteDB.Subsections.Include(s => s.Section).FirstOrDefaultAsync(s => s.Id == subsectionId);
                if (subsection != null)
                {
                    User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);

                    if (subsection.Section.SectionAccessLevel <= user.Role.AccessLevel)
                    {
                        ViewBag.SubsectionId = subsection.Id;
                        return View();
                    }
                }

                return Redirect("/Main/PageNotFound");
            }

            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Создать топик [POST]
        public async Task<IActionResult> CreateTopic(CreateTopicViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {
                    Subsection subsection = await _websiteDB.Subsections.Include(s => s.Section).FirstOrDefaultAsync(s => s.Id == model.SubsectionId);
                    if (subsection != null)
                    {
                        User user = await _websiteDB.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
                        if (user != null && subsection.Section.SectionAccessLevel <= user.Role.AccessLevel)
                        {
                            Topic topic = new Topic()
                            {
                                Id = Guid.NewGuid(),
                                Subsection = subsection,
                                TopicName = model.TopicName,
                                //TopicBody = model.TopicBody,
                                TopicDate = DateTime.Now,
                                //User = user
                            };

                            Reply reply = new Reply()
                            {
                                Id = Guid.NewGuid(),
                                ReplyBody = model.TopicBody,
                                ReplyDate = DateTime.Now,
                                User = user,
                                Topic = topic
                            };

                            await _websiteDB.Topics.AddAsync(topic);
                            await _websiteDB.Replies.AddAsync(reply);
                            await _websiteDB.SaveChangesAsync();

                            return RedirectToAction("ViewTopic", "Forum", new { topicId = topic.Id });
                        }
                    }

                    return Redirect("/Main/PageNotFound");
                }

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }
        #endregion

        #region Ответить [GET]
        public IActionResult AddReply(Guid topicId)
        {
            ViewBag.TopicId = topicId;

            return View();
        }
        #endregion

        #region Ответить [POST]
        [HttpPost]
        public async Task<IActionResult> AddReply(AddReplyViewModel model)
        {
            if (ModelState.IsValid)
            {
                Topic topic = await _websiteDB.Topics.FirstOrDefaultAsync(t => t.Id == model.TopicId);
                if (topic != null)
                {
                    User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
                    if (user != null)
                    {
                        Reply reply = new Reply()
                        {
                            Id = Guid.NewGuid(),
                            ReplyBody = model.ReplyBody,
                            Topic = topic,
                            User = user,
                            ReplyDate = DateTime.Now
                        };

                        await _websiteDB.Replies.AddAsync(reply);
                        await _websiteDB.SaveChangesAsync();

                        return RedirectToAction("ViewTopic", "Forum", new { topicId = topic.Id });
                    }
                }

                return Redirect("/Main/PageNotFound");
            }

            return View(model);
        }
        #endregion

    }
}
