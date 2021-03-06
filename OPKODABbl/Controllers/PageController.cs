﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Models.Account;
using OPKODABbl.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Controllers
{
    public class PageController : Controller
    {
        private readonly WebsiteContext _websiteDB;

        public PageController(WebsiteContext websiteDbContext)
        {
            _websiteDB = websiteDbContext;
        }

        #region Состав гильдии
        public async Task<IActionResult> Roster()
        {
            List<User> users = await _websiteDB.Users.Include(u => u.Role).Include(u => u.CharacterClass).Where(u => u.Role.AccessLevel >= 2 && u.Name != "Administrator").OrderByDescending(u => u.Role.AccessLevel).ToListAsync();

            ViewBag.Title = "Состав гильдии ОРКОДАВЫ";

            return View(users);
        }
        #endregion

    }
}
