using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication
{
    public partial class Shop
    {
        public Shop()
        {
            Orders = new HashSet<Order>();
            ProductsInShops = new HashSet<ProductInShop>();
        }

        public int Id { get; set; }
        [Display(Name = "Назва")]
        [Required(ErrorMessage = "Введіть назву")]
        public string Name { get; set; } = null!;
        [Display(Name = "Адреса")]
        [Required(ErrorMessage = "Введіть адресу")]
        public string Address { get; set; } = null!;
        [Display(Name = "Телефон")]
        [Required(ErrorMessage = "Введіть телефон")]
        public string Phone { get; set; } = null!;
        [Display(Name = "Сайт")]
        [Required(ErrorMessage = "Введіть сайт")]
        public string Site { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProductInShop> ProductsInShops { get; set; }

        public string NameWithAddress => Name + " (" + Address + ") ";
    }
}
