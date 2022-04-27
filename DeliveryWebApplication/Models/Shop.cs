using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication
{
    public partial class Shop : Deletable
    {
        public Shop()
        {
            Orders = new HashSet<Order>();
            ProductsInShops = new HashSet<ProductInShop>();
        }

        public int Id { get; set; }
        [Display(Name = "Назва")]
        [MaxLength(50, ErrorMessage = "Назва магазину не може бути довша, ніж 50 символів")]
        [Required(ErrorMessage = "Введіть назву")]
        public string Name { get; set; } = null!;
        [Display(Name = "Адреса")]
        [MaxLength(200, ErrorMessage = "Адреса не може бути довша, ніж 200 символів")]
        [Required(ErrorMessage = "Введіть адресу")]
        public string Address { get; set; } = null!;
        [Display(Name = "Телефон")]
        [MaxLength(50, ErrorMessage = "Телефон не може бути довший, ніж 50 символів")]
        [Required(ErrorMessage = "Введіть телефон")]
        [RegularExpression(@"^([+]?[\s0-9]+)?(\d{3}|[(]?[0-9]+[)])?([-]?[\s]?[0-9])+$", ErrorMessage = "Введіть коректний телефон")]
        public string Phone { get; set; } = null!;
        [Display(Name = "Сайт")]
        [MaxLength(50, ErrorMessage = "Сайт не може бути довший, ніж 50 символів")]
        [Required(ErrorMessage = "Введіть сайт")]
        [RegularExpression(@"(http(s)?:\/\/.)?(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)", ErrorMessage = "Введіть коректний сайт")]
        public string Site { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProductInShop> ProductsInShops { get; set; }

        public string NameWithAddress => (Deleted ? "* " : "") + Name + " (" + Address + ")" + (Deleted ? " — цей магазин був видалений" : "");

        public bool HasAlive => ProductsInShops.Alive().Any();
        [Display(Name = "Кількість товарів")]
        public int CountAlive => ProductsInShops.Alive().Count();
    }
}
