using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Account
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public int AccessLevel { get; set; }
        public List<User> Users { get; set; }
        public Role()
        {
            Users = new List<User>();
        }
    }
}
