using System;
using System.Collections.Generic;

namespace DeliveryWebApplication
{
    public partial class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductInShopId { get; set; }
        public int Count { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual ProductInShop ProductInShop { get; set; } = null!;
    }
}
