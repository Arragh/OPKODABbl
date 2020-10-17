using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.Forum
{
    public class AddReplyViewModel
    {
        public Guid TopicId { get; set; }

        [Required(ErrorMessage = "Поле ответа не может быть пустым")]
        [Display(Name = "Введите текст сообщения")]
        [DataType(DataType.Text)]
        [StringLength(5000, ErrorMessage = "Логин должен быть от {2} до {1} символов.", MinimumLength = 1)]
        public string ReplyBody { get; set; }
    }
}
