using System;
using System.Collections.Generic;

namespace OPKODABbl.Models.Main
{
    public class News
    {
        public Guid Id { get; set; }
        public string NewsTitle { get; set; }
        public string NewsBody { get; set; }
        public DateTime NewsDate { get; set; }
        public string NewsUserName { get; set; }
        public List<NewsImage> NewsImages { get; set; }
    }
}
