using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.Forum
{
    public class AddReplyViewModel
    {
        public Guid TopicId { get; set; }
        public string ReplyBody { get; set; }
    }
}
