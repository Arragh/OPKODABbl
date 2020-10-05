using OPKODABbl.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Areas.Admin.ViewModels.Users
{
    public class AllUsersViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<Role> Roles { get; set; }
        public IEnumerable<CharacterClass> CharacterClasses { get; set; }
    }
}
