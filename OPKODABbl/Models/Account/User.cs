using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Account
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        public Guid CharacterClassId { get; set; }
        public CharacterClass CharacterClass { get; set; }
        public virtual AvatarImage AvatarImage { get; set; }
    }
}
