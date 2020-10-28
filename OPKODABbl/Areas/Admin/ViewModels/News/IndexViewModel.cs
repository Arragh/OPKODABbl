using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Areas.Admin.ViewModels.News
{
    public class IndexViewModel
    {
        public int MaxImageSize { get; set; }
        public int ImageResizeQuality { get; set; }
        public List<Models.Main.News> News { get; set; }
    }
}
