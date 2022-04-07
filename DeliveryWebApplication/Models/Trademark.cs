using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication
{
    public partial class Trademark
    {
        public Trademark()
        {
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        [Display(Name = "Назва")]
        [Required(ErrorMessage = "Введіть назву")]
        public string Name { get; set; } = null!;

        public virtual ICollection<Product> Products { get; set; }
    }
}
