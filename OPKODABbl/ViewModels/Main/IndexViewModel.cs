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
        public List<Models.Main.News> News { get; set; }
        public List<Models.Gallery.Gallery> Galleries { get; set; }
    }
}
