using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Areas.Admin.ViewModels.Users;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Account;
using OPKODABbl.Service;

namespace OPKODABbl.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private UsersContext _usersDB;
        private IWebHostEnvironment _appEnvironment;

        public UsersController(UsersContext usersDBContext, IWebHostEnvironment appEnvironment)
        {
            _usersDB = usersDBContext;
            _appEnvironment = appEnvironment;
        }

        #region Список пользователей
        public async Task<IActionResult> AllUsers()
        {
            List<User> users = await _usersDB.Users.Include(u => u.Role).OrderByDescending(u => u.Role).ToListAsync();
            List<Role> roles = await _usersDB.Roles.ToListAsync();

            AllUsersViewModel model = new AllUsersViewModel()
            {
                Users = users,
                Roles = roles
            };

            return View(model);
        }
        #endregion

        #region Изменение пользователя [GET]
        public async Task<IActionResult> EditUser(Guid userId)
        {
            User user = await _usersDB.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                SelectList rolesList = new SelectList(_usersDB.Roles, "Id", "Name", user.RoleId);
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
            User user = await _usersDB.Users.FirstAsync(u => u.Id == model.UserId);

            if (user != null)
            {
                user.Name = model.Name;
                user.Email = model.Email;
                user.RoleId = model.RoleId;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password.HashString();
                }

                _usersDB.Users.Update(user);
                await _usersDB.SaveChangesAsync();

                return RedirectToAction("AllUsers", "Users");
            }

            return Redirect("/Main/PageNotFound");
        }
        #endregion

    }
}
