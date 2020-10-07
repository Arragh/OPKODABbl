using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Models.Account;
using OPKODABbl.Service;
using OPKODABbl.ViewModels.Page;

namespace OPKODABbl.Controllers
{
    public class PageController : Controller
    {
        private readonly UsersContext _usersDB;

        public PageController(UsersContext usersDbContext)
        {
            _usersDB = usersDbContext;
        }

        public async Task<IActionResult> Roster()
        {
            //List<User> users = await _usersDB.Users.Include(u => u.Role).Where(u => u.Role.Name == "clanmember" || u.Role.Name == "admin" || u.Role.Name == "recruit").OrderBy(u => u.Role.Name).ToListAsync();

            List<User> users = await _usersDB.Users.Include(u => u.Role).Where(u => u.Role.AccessLevel >= 2 && u.Name != "Administrator").OrderByDescending(u => u.Role.AccessLevel).ToListAsync();

            List<CharacterClass> characterClasses = await _usersDB.CharacterClasses.ToListAsync();

            RosterViewModel model = new RosterViewModel()
            {
                Users = users,
                CharacterClasses = characterClasses
            };

            return View(model);
        }
    }
}
