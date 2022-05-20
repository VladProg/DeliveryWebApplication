using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введіть e-mail")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
