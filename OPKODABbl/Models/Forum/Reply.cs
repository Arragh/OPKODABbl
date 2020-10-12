using OPKODABbl.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Forum
{
    public class Reply
    {
        public Guid Id { get; set; }
        public string ReplyBody { get; set; }
        public Guid TopicId { get; set; }
        public DateTime ReplyDate { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
