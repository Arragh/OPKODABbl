﻿using OPKODABbl.Models.Account;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.ViewModels.Account
{
    public class EditProfileViewModel
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Укажите логин")]
        [Display(Name = "Логин пользователя")]
        [DataType(DataType.Text)]
        [StringLength(64, ErrorMessage = "Логин должен быть от {2} до {1} символов.", MinimumLength = 6)]
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

        [Display(Name = "Имя персонажа")]
        [DataType(DataType.Text)]
        public string CharacterName { get; set; }

        [Display(Name = "Класс")]
        [DataType(DataType.Text)]
        public string CharacterClass { get; set; }

        public Guid RoleId { get; set; }
        
        public string AvatarImage { get; set; }
    }
}
