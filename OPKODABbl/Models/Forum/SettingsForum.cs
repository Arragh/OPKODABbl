using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Forum
{
    public class SettingsForum
    {
        public Guid Id { get; set; }
        public int SubsectionPageSize { get; set; }
        public int TopicPageSize { get; set; }
    }
}
