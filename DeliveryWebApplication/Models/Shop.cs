using System;
using System.Collections.Generic;

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
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Site { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProductInShop> ProductsInShops { get; set; }

        public string NameWithAddress => Name + " (" + Address + ") ";
    }
}
