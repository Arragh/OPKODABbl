using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Gallery
{
    public class SettingsGallery
    {
        public Guid Id { get; set; }
        public int GalleriesPerPage { get; set; }
        public int ImagesPerGallery { get; set; }
        public int MaxImageSize { get; set; }
        public int ImageResizeQuality { get; set; }
    }
}
