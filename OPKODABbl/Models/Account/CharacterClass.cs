using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Account
{
    public class CharacterClass
    {
        public Guid Id { get; set; }
        public string ClassIconPath { get; set; }
        public string ClassName { get; set; }
        public string ClassColor { get; set; }
    }
}
