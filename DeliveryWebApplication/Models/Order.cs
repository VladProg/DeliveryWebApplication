using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryWebApplication
{
    public partial class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public const decimal MINIMAL_DELIVERY_PRICE = 10;
        public const decimal MAXIMAL_DELIVERY_PRICE = 1000000;
        public const string DELIVERY_PRICE_MESSAGE = "Ціна повинна бути в межах від 10 до 1000000 гривень";

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
        [Range((double)MINIMAL_DELIVERY_PRICE, (double)MAXIMAL_DELIVERY_PRICE, ErrorMessage = DELIVERY_PRICE_MESSAGE)]
        [RegularExpression(@"\d+(\.\d+)?", ErrorMessage = "Введіть коректне число")]
        public decimal? DeliveryPrice { get; set; }
        [Display(Name = "Час створення")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy HH:mm:ss}")]
        public DateTime? CreationTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy HH:mm:ss}")]
        [Display(Name = "Час доставки")]
        public DateTime? DeliveryTime { get; set; }
        [Display(Name = "Адреса доставки")]
        [Required(ErrorMessage = "Введіть адресу")]
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

        public enum Status { None, Delivering, Waiting, Refused, Creating, Completed };

        public Status StatusId
        {
            get
            {
                if (CreationTime is null) return Status.Creating;
                if (CourierComment is not null) return Status.Refused;
                if (CourierId is null) return Status.Waiting;
                if (DeliveryTime is null) return Status.Delivering;
                return Status.Completed;
            }
        }

        public struct IconWithName
        {
            public string Icon;
            public string Name;
            public string Full => Icon + " " + Name;
            public IconWithName(string icon, string name)
            {
                Icon = icon;
                Name = name;
            }
        }

        public static readonly IconWithName[] STATUS_NAMES =
        {
            new IconWithName("",""),
            new IconWithName("🚚","Кур'єр доставляє замовлення"),
            new IconWithName("🔍","Шукаємо кур'єра для доставки замовлення"),
            new IconWithName("❌","Кур'єр відмовився доставляти замовлення"),
            new IconWithName("📝","Клієнт формує замовлення"),
            new IconWithName("✅","Замовлення доставлене")
        };

        [Display(Name = "Статус")]
        public IconWithName StatusName => STATUS_NAMES[(int)StatusId];

        [Display(Name = "Вартість товарів")]
        [DisplayFormat(DataFormatString = "{0:n2} ₴")]
        public decimal ProductsCost => OrderItems.Select(oi => oi.Cost).Sum();

        [Display(Name = "Загальна вартість")]
        [DisplayFormat(DataFormatString = "{0:n2} ₴")]
        public decimal? TotalCost => ProductsCost + DeliveryPrice;

        [Display(Name = "Вага")]
        public decimal Weight => OrderItems.Sum(oi => (decimal)oi.Weight);
        [Display(Name = "Вага")]
        public string FormattedWeight => Utils.FormattedWeight(Weight);

        public string Description => "#" + Id + (Customer is null ? "" : " — " + Customer.NameWithPhone);
    }
}
