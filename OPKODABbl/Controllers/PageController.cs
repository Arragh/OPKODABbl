using Microsoft.AspNetCore.Mvc;
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
        private readonly UsersContext _usersDB;

        public PageController(UsersContext usersDbContext)
        {
            _usersDB = usersDbContext;
        }

        #region Состав гильдии
        public async Task<IActionResult> Roster()
        {
            List<User> users = await _usersDB.Users.Include(u => u.Role).Include(u => u.CharacterClass).Where(u => u.Role.AccessLevel >= 2 && u.Name != "Administrator").OrderByDescending(u => u.Role.AccessLevel).ToListAsync();

            return View(users);
        }
        #endregion

    }
}
