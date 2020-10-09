using System;
using System.Collections.Generic;

namespace OPKODABbl.Models.Gallery
{
    public class Gallery
    {
        public Guid Id { get; set; }
        public string GalleryTitle { get; set; }
        public string GalleryDescription { get; set; }
        public DateTime GalleryDate { get; set; }
        public string GalleryUserName { get; set; }
        public string GallerySliderImage { get; set; }
        public List<GalleryImage> GalleryImages { get; set; }
    }
}
