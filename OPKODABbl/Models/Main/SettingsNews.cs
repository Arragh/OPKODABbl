using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Main
{
    public class SettingsNews
    {
        public Guid Id { get; set; }
        public int MaxImageSize { get; set; }
        public int ImageResizeQuality { get; set; }
    }
}
