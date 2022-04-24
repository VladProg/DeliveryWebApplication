using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication
{
    public partial class ProductInShop : Deletable
    {
        public ProductInShop()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        [Display(Name = "Магазин")]
        [Required(ErrorMessage = "Оберіть магазин")]
        public int ShopId { get; set; }
        [Display(Name = "Ціна")]
        [Required(ErrorMessage = "Введіть ціну")]
        [Range(0.01, 1000000, ErrorMessage = "Вага повинна бути в межах від 0.01 до 1000000 гривень")]
        [RegularExpression(@"\d+(\.\d+)?", ErrorMessage = "Введіть коректне число")]
        [DisplayFormat(DataFormatString = "{0:n2} ₴")]
        public decimal Price { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Shop Shop { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; }

        [Display(Name = "Ціна")]
        public string FormattedPrice
        {
            get
            {
                if (Product is null)
                    return "null";
                if (Product.Weight is null)
                    return Price.ToString("0.00") + " ₴/кг";
                else
                    return Price.ToString("0.00") + " ₴";
            }
        }
    }
}
