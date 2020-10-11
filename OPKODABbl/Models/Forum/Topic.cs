﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Forum
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string TopicName { get; set; }
        public string TopicBody { get; set; }
        public Guid UserId { get; set; }
        public Guid SubsectionId { get; set; }
        public Subsection Subsection { get; set; }
    }
}
