using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Forum
{
    public class Section
    {
        public Guid Id { get; set; }
        public string SectionName { get; set; }
        public int SectionPosition { get; set; }
        public List<Subsection> Subsections { get; set; }
    }
}
