using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Models.Gallery
{
    public class GalleryImage
    {
        public Guid Id { get; set; }
        public string ImageName { get; set; }
        public string ImagePathOriginal { get; set; }
        public string ImagePathScaled { get; set; }
        public DateTime ImageDate { get; set; }
        public Guid GalleryId { get; set; }
        public virtual Gallery Gallery { get; set; }
    }
}
