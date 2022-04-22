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
        [Display(Name = "Клієнт")]
        [Required(ErrorMessage = "Оберіть клієнта")]
        public int CustomerId { get; set; }
        [Display(Name = "Кур'єр")]
        public int? CourierId { get; set; }
        [Display(Name = "Магазин")]
        [Required(ErrorMessage = "Оберіть магазин")]
        public int ShopId { get; set; }
        [Display(Name = "Ціна доставки")]
        [DisplayFormat(DataFormatString = "{0:n2} ₴")]
        public decimal? DeliveryPrice { get; set; }
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

        public enum Status { None, Waiting, Refused, Delivering, Creating, Completed };

        public Status StatusId
        {
            get
            {
                if (CreationTime is null) return Status.Creating;
                if (CourierId is null && CourierComment is null) return Status.Waiting;
                if (DeliveryTime is null) return Status.Delivering;
                if (CourierId is null) return Status.Refused;
                return Status.Completed;
            }
        }

        public static readonly string[] STATUS_NAMES =
        {
            "",
            "Шукаємо кур'єра для доставки замовлення",
            "Кур'єр відмовився доставляти замовлення",
            "Кур'єр доставляє замовлення",
            "Клієнт формує замовлення",
            "Замовлення вже доставлене"
        };

        [Display(Name = "Статус")]
        public string StatusName => STATUS_NAMES[(int)StatusId];

        [Display(Name = "Вартість продуктів")]
        [DisplayFormat(DataFormatString = "{0:n2} ₴")]
        public decimal ProductsCost => OrderItems.Select(oi => oi.Cost).Sum();

        [Display(Name = "Загальна вартість")]
        [DisplayFormat(DataFormatString = "{0:n2} ₴")]
        public decimal? TotalCost => ProductsCost + DeliveryPrice;
    }
}
