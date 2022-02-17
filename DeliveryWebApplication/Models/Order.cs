using System;
using System.Collections.Generic;

namespace DeliveryWebApplication
{
    public partial class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? CourierId { get; set; }
        public int ShopId { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public string? Address { get; set; }
        public string? CustomerComment { get; set; }
        public string? CourierComment { get; set; }

        public virtual Courier? Courier { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        public virtual Shop Shop { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
