using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.Forum
{
    public class CreateTopicViewModel
    {
        public Guid SubsectionId { get; set; }

        [Required(ErrorMessage = "Заголовок темы не может быть пустым")]
        [Display(Name = "Заголовок темы")]
        [DataType(DataType.Text)]
        [StringLength(120, ErrorMessage = "Заголовок темы должен быть от {2} до {1} символов.", MinimumLength = 4)]
        public string TopicName { get; set; }

        [Required(ErrorMessage = "Поле сообщения не может быть пустым")]
        [Display(Name = "Введите текст сообщения")]
        [DataType(DataType.Text)]
        [StringLength(5000, ErrorMessage = "Текст сообщения должен быть от {2} до {1} символов.", MinimumLength = 4)]
        public string TopicBody { get; set; }

        [Display(Name = "Объявление")]
        public bool Announcement { get; set; }
    }
}
