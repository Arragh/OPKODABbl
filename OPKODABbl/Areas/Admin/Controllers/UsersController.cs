using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using OPKODABbl.Areas.Admin.ViewModels.Users;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Account;
using OPKODABbl.Models.Forum;
using OPKODABbl.Service;

namespace OPKODABbl.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private WebsiteContext _websiteDB;
        private IWebHostEnvironment _appEnvironment;

        public UsersController(WebsiteContext websiteDBContext, IWebHostEnvironment appEnvironment)
        {
            _websiteDB = websiteDBContext;
            _appEnvironment = appEnvironment;
        }

        #region Список пользователей
        public async Task<IActionResult> AllUsers()
        {
            //List<User> users = await _websiteDB.Users.Include(u => u.Role).OrderByDescending(u => u.Role.AccessLevel).ThenBy(u => u.RegisterDate).ToListAsync();
            //List<Role> roles = await _websiteDB.Roles.ToListAsync();
            //List<CharacterClass> characterClasses = await _websiteDB.CharacterClasses.ToListAsync();

            //AllUsersViewModel model = new AllUsersViewModel()
            //{
            //    Users = users,
            //    Roles = roles,
            //    CharacterClasses = characterClasses
            //};

            //return View(model);


            List<User> users = await _websiteDB.Users.Include(u => u.Role).Include(u => u.CharacterClass).OrderByDescending(u => u.Role.AccessLevel).ThenBy(u => u.RegisterDate).ToListAsync();

            return View(users);
        }
        #endregion

        #region Изменение пользователя [GET]
        public async Task<IActionResult> EditUser(Guid userId)
        {
            User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                // Список ролей для выпадающего списка droplist на странице
                SelectList rolesList = new SelectList(_websiteDB.Roles.OrderByDescending(r => r.AccessLevel), "Id", "Name", user.RoleId);
                ViewBag.Roles = rolesList;

                EditUserViewModel model = new EditUserViewModel()
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    RoleId = user.RoleId
                };

                return View(model);
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Изменение пользователя [POST]
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            User user = await _websiteDB.Users.FirstAsync(u => u.Id == model.UserId);

            if (user != null)
            {
                // Список ролей для выпадающего списка droplist на странице
                SelectList rolesList = new SelectList(_websiteDB.Roles.OrderByDescending(r => r.AccessLevel), "Id", "Name", user.RoleId);
                ViewBag.Roles = rolesList;

                user.Name = model.Name;
                user.Email = model.Email;
                user.RoleId = model.RoleId;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password.HashString();
                }

                _websiteDB.Users.Update(user);
                await _websiteDB.SaveChangesAsync();

                return RedirectToAction("AllUsers", "Users");
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

        #region Удалить пользователя со всеми его сообщениями[POST]
        [HttpPost]
        public async Task<IActionResult> DeleteUserWith(Guid userId, bool isChecked)
        {
            User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null && isChecked)
            {
                List<Reply> replies = await _websiteDB.Replies.Include(r => r.User).Where(r => r.User.Id == user.Id).ToListAsync();
                List<Topic> topics = await _websiteDB.Topics.Include(t => t.User).Include(t => t.Replies).Where(t => t.User.Id == user.Id).ToListAsync();

                _websiteDB.Replies.RemoveRange(topics.SelectMany(t => t.Replies).ToList());
                _websiteDB.Replies.RemoveRange(replies);
                _websiteDB.Topics.RemoveRange(topics);
                _websiteDB.Users.Remove(user);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("AllUsers", "Users");
        }
        #endregion

        #region Удалить пользователя и оставить сообщения[POST]
        [HttpPost]
        public async Task<IActionResult> DeleteUserWithout(Guid userId, bool isChecked)
        {
            User user = await _websiteDB.Users.Include(u => u.Topics).ThenInclude(t => t.Replies)
                                              .Include(u => u.Replies).FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && isChecked)
            {
                foreach (var reply in user.Replies)
                {
                    reply.User = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == "Anonymous");
                }

                //foreach (var topic in user.Topics)
                //{
                //    topic.User = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == "Anonymous");
                //}

                _websiteDB.Users.Remove(user);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("AllUsers", "Users");
        }
        #endregion

    }
}
