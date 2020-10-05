using OPKODABbl.Models.Main;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Areas.Admin.ViewModels.News
{
    public class EditNewsViewModel
    {
        [Required(ErrorMessage = "Требуется ввести заголовок.")]
        [Display(Name = "Заголовок")]
        [StringLength(120, ErrorMessage = "Заголовок должен быть от {2} до {1} символов.", MinimumLength = 4)]
        [DataType(DataType.Text)]
        public string NewsTitle { get; set; }

        [Required(ErrorMessage = "Требуется ввести содержание")]
        [Display(Name = "Содержание")]
        [StringLength(5000, ErrorMessage = "Содержание должно быть от {2} до {1} символов.", MinimumLength = 10)]
        [DataType(DataType.Text)]
        public string NewsBody { get; set; }

        [Display(Name = "Загрузить изображение")]
        public NewsImage NewsImage { get; set; }


        public Guid NewsId { get; set; }

        public DateTime NewsDate { get; set; }

        public string NewsUserName { get; set; }

        public List<NewsImage> NewsImages { get; set; }

        public int NewsImagesCount { get; set; }
    }
}
