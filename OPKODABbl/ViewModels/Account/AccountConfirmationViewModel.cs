using System.ComponentModel.DataAnnotations;

namespace OPKODABbl.ViewModels.Account
{
    public class AccountConfirmationViewModel
    {
        [Required(ErrorMessage = "Введите Email")]
        [Display(Name = "Адрес Email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Введите корректный адрес Email")]
        public string Email { get; set; }
    }
}
