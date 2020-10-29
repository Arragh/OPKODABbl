using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Areas.Admin.ViewModels.Captcha
{
    public class AddCaptchaViewModel
    {
        [Required]
        [Display(Name = "Вопрос к пользователю")]
        public string Question { get; set; }

        [Required]
        [Display(Name = "Ответ на вопрос")]
        public string Answer { get; set; }

        public string CaptchaImage { get; set; }
    }
}
