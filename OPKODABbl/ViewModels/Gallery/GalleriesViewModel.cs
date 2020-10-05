using OPKODABbl.Models.Gallery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.Gallery
{
    public class GalleriesViewModel
    {
        public List<Models.Gallery.Gallery> Galleries { get; set; }
        public Dictionary<Guid, string> PreviewImages { get; set; }
    }
}
