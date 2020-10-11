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
        private readonly ForumContext _forumDB;
        private readonly UsersContext _usersDB;

        public ForumController(ForumContext forumContext, UsersContext usersContext)
        {
            _forumDB = forumContext;
            _usersDB = usersContext;
        }

        #region Главная страница форума
        public async Task<IActionResult> Index()
        {
            List<Section> sections = await _forumDB.Sections.Include(s => s.Subsections).OrderBy(s => s.SectionPosition).ToListAsync();

            return View(sections);
        }
        #endregion

        #region Просмотр раздела форума
        public async Task<IActionResult> Subsection(Guid subsectionId)
        {
            Subsection subsection = await _forumDB.Subsections.Include(s => s.Topics).FirstOrDefaultAsync(s => s.Id == subsectionId);
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
            Subsection subsection = await _forumDB.Subsections.FirstOrDefaultAsync(s => s.Id == subsectionId);
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
            Subsection subsection = await _forumDB.Subsections.Include(s => s.Topics).FirstOrDefaultAsync(s => s.Id == model.SubsectionId);
            if (subsection != null)
            {
                User user = await _usersDB.Users.FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
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

                    await _forumDB.Topics.AddAsync(topic);
                    await _forumDB.SaveChangesAsync();

                    return RedirectToAction("ViewTopic", "Forum", new { topicId = topic.Id });
                }
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Просмотр топика
        public async Task<IActionResult> ViewTopic(Guid topicId)
        {
            Topic topic = await _forumDB.Topics.FirstOrDefaultAsync(t => t.Id == topicId);
            if (topic != null)
            {
                return View(topic);
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

    }
}
