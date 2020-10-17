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
            List<User> users = await _websiteDB.Users.Include(u => u.Role).OrderBy(u => u.RegisterDate).ToListAsync();
            List<Role> roles = await _websiteDB.Roles.ToListAsync();
            List<CharacterClass> characterClasses = await _websiteDB.CharacterClasses.ToListAsync();

            AllUsersViewModel model = new AllUsersViewModel()
            {
                Users = users,
                Roles = roles,
                CharacterClasses = characterClasses
            };

            return View(model);
        }
        #endregion

        #region Изменение пользователя [GET]
        public async Task<IActionResult> EditUser(Guid userId)
        {
            User user = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
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
        public async Task<IActionResult> DeleteUserWith(Guid userId)
        {
            User user = await _websiteDB.Users.Include(u => u.Topics).ThenInclude(t => t.Replies)
                                              .Include(u => u.Replies).FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                _websiteDB.Replies.RemoveRange(user.Replies);

                foreach (var topic in user.Topics)
                {
                    _websiteDB.RemoveRange(topic.Replies);
                }
                _websiteDB.Topics.RemoveRange(user.Topics);
                _websiteDB.Users.Remove(user);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("AllUsers", "Users");
        }
        #endregion

        #region Удалить пользователя и оставить сообщения[POST]
        public async Task<IActionResult> DeleteUserWithout(Guid userId)
        {
            User user = await _websiteDB.Users.Include(u => u.Topics).ThenInclude(t => t.Replies)
                                              .Include(u => u.Replies).FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                foreach (var reply in user.Replies)
                {
                    reply.User = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == "Anonymous");
                }

                foreach (var topic in user.Topics)
                {
                    topic.User = await _websiteDB.Users.FirstOrDefaultAsync(u => u.Name == "Anonymous");
                }

                _websiteDB.Users.Remove(user);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("AllUsers", "Users");
        }
        #endregion

    }
}
