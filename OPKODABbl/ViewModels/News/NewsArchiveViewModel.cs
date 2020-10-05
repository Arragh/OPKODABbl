using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.News
{
    public class NewsArchiveViewModel
    {
        public List<Models.Main.News> News { get; set; }
        public Dictionary<Guid, string> PreviewImages { get; set; }
    }
}
