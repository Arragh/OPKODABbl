﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Forum
{
    public class Subsection
    {
        public Guid Id { get; set; }
        public string SubsectionName { get; set; }
        public int SubsectionPosition { get; set; }
        public Section Section { get; set; }
        public List<Topic> Topics { get; set; }
    }
}
