using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication
{
    public partial class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        [Display(Name = "Номер")]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? CourierId { get; set; }
        public int ShopId { get; set; }
        [Display(Name = "Ціна доставки")]
        public decimal DeliveryPrice { get; set; }
        [Display(Name = "Час створення")]
        public DateTime? CreationTime { get; set; }
        [Display(Name = "Час доставки")]
        public DateTime? DeliveryTime { get; set; }
        [Display(Name = "Адреса")]
        public string Address { get; set; }
        [Display(Name = "Коментар клієнта")]
        public string CustomerComment { get; set; }
        [Display(Name = "Коментар кур'єра")]
        public string? CourierComment { get; set; }

        [Display(Name = "Кур'єр")]
        public virtual Courier? Courier { get; set; }
        [Display(Name = "Клієнт")]
        public virtual Customer Customer { get; set; } = null!;
        [Display(Name = "Магазин")]
        public virtual Shop Shop { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public int StateCode
        {
            get
            {
                if (CreationTime is null) return 0;
                if (CourierId is null && CourierComment is null) return 1;
                if (DeliveryTime is null) return 2;
                if (CourierId is null) return 3;
                return 4;
            }
        }

        [Display(Name = "Статус")]
        public string StateName => new string[]
        {
            "Клієнт формує замовлення",
            "Шукаємо кур'єра для доставки замовлення",
            "Кур'єр доставляє замовлення",
            "Кур'єр відмовився доставляти замовлення",
            "Замовлення вже доставлене"
        }[StateCode];

        [Display(Name = "Вартість продуктів")]
        public decimal ProductsCost => OrderItems.Select(oi => oi.Cost).Sum();

        [Display(Name = "Загальна вартість")]
        public decimal? TotalCost => ProductsCost + DeliveryPrice;
    }
}
