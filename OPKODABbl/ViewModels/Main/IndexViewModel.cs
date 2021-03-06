﻿using OPKODABbl.Models.Forum;
using System.Collections.Generic;

namespace OPKODABbl.ViewModels.Main
{
    public class IndexViewModel
    {
        public List<Models.Main.News> News { get; set; }
        public List<Topic> Topics { get; set; }
        public List<Models.Gallery.Gallery> Galleries { get; set; }
    }
}
