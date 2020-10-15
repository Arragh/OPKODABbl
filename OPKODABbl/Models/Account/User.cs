using OPKODABbl.Models.Forum;
using System;
using System.Collections.Generic;

namespace OPKODABbl.Models.Account
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool IsConfirmed { get; set; }
        public string ConfirmationKey { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        public Guid CharacterClassId { get; set; }
        public CharacterClass CharacterClass { get; set; }
        public List<Topic> Topics { get; set; }
        public List<Reply> Replies { get; set; }
        public AvatarImage AvatarImage { get; set; }
    }
}
