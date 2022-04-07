using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication
{
    public partial class ProductInShop
    {
        public ProductInShop()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ShopId { get; set; }
        [Display(Name = "Ціна")]
        [Required(ErrorMessage = "Введіть ціну")]
        public decimal Price { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Shop Shop { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
