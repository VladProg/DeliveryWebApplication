using System;
using System.Collections.Generic;

namespace DeliveryWebApplication
{
    public partial class Product
    {
        public Product()
        {
            ProductsInShops = new HashSet<ProductInShop>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Weight { get; set; }
        public int? TrademarkId { get; set; }
        public int CategoryId { get; set; }
        public int CountryId { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual Country Country { get; set; } = null!;
        public virtual Trademark? Trademark { get; set; }
        public virtual ICollection<ProductInShop> ProductsInShops { get; set; }
    }
}
