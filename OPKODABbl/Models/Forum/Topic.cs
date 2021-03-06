﻿using OPKODABbl.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Forum
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string TopicName { get; set; }
        public DateTime TopicDate { get; set; }
        public int Views { get; set; }
        public bool Announcement { get; set; }
        public Subsection Subsection { get; set; }
        public User User { get; set; }
        public List<Reply> Replies { get; set; }
    }
}
