using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication
{
    public partial class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        [Display(Name = "Назва")]
        [MaxLength(50, ErrorMessage = "Назва категорії не може бути довша, ніж 50 символів")]
        [Required(ErrorMessage = "Введіть назву категорії")]
        public string Name { get; set; } = null!;
        public bool Deleted { get; set; } = false;

        public virtual ICollection<Product> Products { get; set; }

        public int Count => Products.Count;
    }
}
