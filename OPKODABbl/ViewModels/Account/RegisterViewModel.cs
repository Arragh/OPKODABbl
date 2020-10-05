using System.ComponentModel.DataAnnotations;

namespace OPKODABbl.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин пользователя")]
        [DataType(DataType.Text)]
        [StringLength(64, ErrorMessage = "Логин должен быть от {2} до {1} символов.", MinimumLength = 6)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите Email")]
        [Display(Name = "Адрес Email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Введите корректный адрес Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Задайте пароль")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(64, ErrorMessage = "Пароль должен быть от {2} до {1} символов.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Повторите пароль")]
        [Display(Name = "Повторите пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string PasswordConfirm { get; set; }
    }
}
