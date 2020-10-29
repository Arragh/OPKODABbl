using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Account
{
    public class Captcha
    {
        public Guid Id { get; set; }
        public string ImagePathOriginal { get; set; }
        public string ImagePathScaled { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
