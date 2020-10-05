﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Account
{
    public class AvatarImage
    {
        public Guid Id { get; set; }
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
        public DateTime ImageDate { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}
