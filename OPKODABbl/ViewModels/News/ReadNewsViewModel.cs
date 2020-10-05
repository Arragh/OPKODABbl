using OPKODABbl.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.News
{
    public class ReadNewsViewModel
    {
        public Models.Main.News News { get; set; }
        public IEnumerable<NewsImage> NewsImages { get; set; }
    }
}
