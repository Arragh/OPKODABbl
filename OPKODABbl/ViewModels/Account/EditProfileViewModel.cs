using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace OPKODABbl.ViewModels.Account
{
    public class EditProfileViewModel
    {
        public Guid UserId { get; set; }

        public string Name { get; set; }

        [Required(ErrorMessage = "Укажите Email")]
        [Display(Name = "Адрес Email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Введите корректный адрес Email")]
        public string Email { get; set; }

        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(64, ErrorMessage = "Пароль должен быть от {2} до {1} символов.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Display(Name = "Повторите пароль")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают.")]
        public string NewPasswordConfirm { get; set; }

        [Required(ErrorMessage = "Укажите текущий пароль")]
        [Display(Name = "Текущий пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Укажите класс вашего персонажа в игре")]
        [Display(Name = "Класс игрового персонажа")]
        public Guid CharacterClassId { get; set; }

        public string AvatarImage { get; set; }
    }
}
