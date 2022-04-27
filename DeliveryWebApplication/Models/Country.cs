using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication
{
    public partial class Country : Deletable
    {
        public Country()
        {
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        [Display(Name = "Назва")]
        [MaxLength(50, ErrorMessage = "Назва країни не може бути довша, ніж 50 символів")]
        [Required(ErrorMessage = "Введіть назву країни")]
        public string Name { get; set; } = null!;

        public virtual ICollection<Product> Products { get; set; }

        public bool HasAlive => Products.Alive().Any();
        [Display(Name = "Кількість товарів")]
        public int CountAlive => Products.Alive().Count();
    }
}
