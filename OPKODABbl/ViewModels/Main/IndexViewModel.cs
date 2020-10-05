using OPKODABbl.Models.Gallery;
using OPKODABbl.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.Main
{
    public class IndexViewModel
    {
        public IEnumerable<Models.Main.News> News { get; set; }
        public IEnumerable<NewsImage> NewsImages { get; set; }
        public IEnumerable<Models.Gallery.Gallery> Galleries { get; set; }
    }
}
