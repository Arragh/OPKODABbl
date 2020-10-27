using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Areas.Admin.ViewModels.Gallery
{
    public class IndexViewModel
    {
        public int GalleriesPerPage { get; set; }
        public int ImagesPerGallery { get; set; }
        public int MaxImageSize { get; set; }
        public int ImageResizeQuality { get; set; }
        public List<Models.Gallery.Gallery> Galleries { get; set; }
    }
}
