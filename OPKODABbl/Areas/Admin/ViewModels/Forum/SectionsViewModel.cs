using OPKODABbl.Models.Forum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Areas.Admin.ViewModels.Forum
{
    public class SectionsViewModel
    {
        public int SubsectionPageSize { get; set; }
        public int TopicPageSize { get; set; }
        public List<Section> Sections { get; set; }
    }
}
