using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введіть старий пароль")]
        [Display(Name = "Старий пароль")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Введіть новий пароль")]
        [Display(Name = "Новий пароль")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Паролі не співпадають")]
        [Display(Name = "Підтвердження паролю")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }
    }
}
