using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication.ViewModels
{
    public class EditAccountViewModel
    {
        [Display(Name = "Ім'я")]
        [MaxLength(50, ErrorMessage = "Ім'я не може бути довше, ніж 50 символів")]
        [Required(ErrorMessage = "Введіть ім'я")]
        public string Name { get; set; }

        [Display(Name = "Телефон")]
        [MaxLength(50, ErrorMessage = "Телефон не може бути довший, ніж 50 символів")]
        [Required(ErrorMessage = "Введіть телефон")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Введіть коректний телефон у форматі +380XXXXXXXXX")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Введіть E-mail")]
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
