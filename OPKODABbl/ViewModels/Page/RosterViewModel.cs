using OPKODABbl.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.Page
{
    public class RosterViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<CharacterClass> CharacterClasses { get; set; }
    }
}
