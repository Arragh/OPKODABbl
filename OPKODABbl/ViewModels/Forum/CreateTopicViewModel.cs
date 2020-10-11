using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.Forum
{
    public class CreateTopicViewModel
    {
        public Guid SubsectionId { get; set; }
        public string TopicName { get; set; }
        public string TopicBody { get; set; }
    }
}
