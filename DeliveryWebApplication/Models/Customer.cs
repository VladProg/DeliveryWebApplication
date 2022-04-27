using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication
{
    public partial class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        [Display(Name = "Ім'я")]
        [MaxLength(50, ErrorMessage = "Ім'я не може бути довше, ніж 50 символів")]
        [Required(ErrorMessage = "Введіть ім'я")]
        public string Name { get; set; } = null!;
        [Display(Name = "Телефон")]
        [MaxLength(50, ErrorMessage = "Телефон не може бути довший, ніж 50 символів")]
        [Required(ErrorMessage = "Введіть телефон")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Введіть коректний телефон у форматі +380XXXXXXXXX")]
        public string Phone { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; }

        public string NameWithPhone => Name + ", " + Phone;
    }
}
