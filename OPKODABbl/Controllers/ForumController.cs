using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            List<Section> sections = await _websiteDB.Sections.Include(s => s.Subsections).OrderBy(s => s.SectionPosition).ToListAsync();
            foreach (var section in sections)
            {
                section.Subsections.OrderBy(s => s.SubsectionPosition);
            }

            return View(sections);
        }
        #endregion

        #region Просмотр раздела форума
        public async Task<IActionResult> Subsection(Guid subsectionId)
        {
            Subsection subsection = await _websiteDB.Subsections.Include(s => s.Topics).FirstOrDefaultAsync(s => s.Id == subsectionId);
            if (subsection != null)
            {
                ViewBag.SubsectionId = subsection.Id;
                return View(subsection);
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Создать топик[GET]
        [HttpGet]
        public async Task<IActionResult> CreateTopic(Guid subsectionId)
        {
            Subsection subsection = await _websiteDB.Subsections.FirstOrDefaultAsync(s => s.Id == subsectionId);
            if (subsection != null)
            {
                ViewBag.SubsectionId = subsection.Id;
                return View();
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Создать топик [POST]
        public async Task<IActionResult> CreateTopic(CreateTopicViewModel model)
        {
            Subsection subsection = await _websiteDB.Subsections.Include(s => s.Topics).FirstOrDefaultAsync(s => s.Id == model.SubsectionId);
            if (subsection != null)
            {
                User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
                if (user != null)
                {
                    Topic topic = new Topic()
                    {
                        Id = Guid.NewGuid(),
                        SubsectionId = model.SubsectionId,
                        TopicName = model.TopicName,
                        TopicBody = model.TopicBody,
                        UserId = user.Id
                    };

                    await _websiteDB.Topics.AddAsync(topic);
                    await _websiteDB.SaveChangesAsync();

                    return RedirectToAction("ViewTopic", "Forum", new { topicId = topic.Id });
                }
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Просмотр топика
        public async Task<IActionResult> ViewTopic(Guid topicId, int page = 1)
        {
            Topic topic = await _websiteDB.Topics.Include(t => t.User).ThenInclude(u => u.AvatarImage)
                                                 .Include(t => t.Replies).ThenInclude(r => r.User).ThenInclude(u => u.AvatarImage)
                                                 .Include(t => t.Subsection)
                                                 .FirstOrDefaultAsync(t => t.Id == topicId);

            if (topic != null)
            {
                int count = topic.Replies.Count();
                int pageSize = 10;
                topic.Replies = topic.Replies.OrderBy(r => r.ReplyDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
                ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
                ViewBag.CurrentPage = page;

                return View(topic);
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Ответить
        [HttpPost]
        public async Task<IActionResult> AddReply(AddReplyViewModel model)
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
                        TopicId = model.TopicId,
                        UserId = user.Id,
                        ReplyDate = DateTime.Now
                    };

                    await _websiteDB.Replies.AddAsync(reply);
                    await _websiteDB.SaveChangesAsync();

                    return RedirectToAction("ViewTopic", "Forum", new { topicId = topic.Id });
                }
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

    }
}
