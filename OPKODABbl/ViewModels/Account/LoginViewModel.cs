using System.ComponentModel.DataAnnotations;

namespace OPKODABbl.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин пользователя")]
        [DataType(DataType.Text)]
        [StringLength(64, ErrorMessage = "Логин должен быть от {2} до {1} символов.", MinimumLength = 6)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(64, ErrorMessage = "Пароль должен быть от {2} до {1} символов.", MinimumLength = 6)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
