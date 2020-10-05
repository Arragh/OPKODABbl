using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Main
{
    public class News
    {
        public Guid Id { get; set; }
        public string NewsTitle { get; set; }
        public string NewsBody { get; set; }
        public DateTime NewsDate { get; set; }
        public string NewsUserName { get; set; }
        public virtual ICollection<NewsImage> NewsImages { get; set; }
    }
}
