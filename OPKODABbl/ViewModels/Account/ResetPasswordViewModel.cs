﻿using System.ComponentModel.DataAnnotations;

namespace OPKODABbl.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин пользователя")]
        [DataType(DataType.Text)]
        [StringLength(64, ErrorMessage = "Логин должен быть от {2} до {1} символов.", MinimumLength = 4)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите Email")]
        [Display(Name = "Адрес Email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Введите корректный адрес Email")]
        public string Email { get; set; }
    }
}
