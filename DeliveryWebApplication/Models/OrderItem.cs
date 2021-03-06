using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryWebApplication
{
    public partial class OrderItem
    {
        public int Id { get; set; }
        [Display(Name = "Замовлення")]
        public int OrderId { get; set; }
        public int ProductInShopId { get; set; }
        [Display(Name = "Кількість")]
        public int Count { get; set; }

        public virtual Order Order { get; set; } = null!;
        [Display(Name = "Товар")]
        public virtual ProductInShop ProductInShop { get; set; } = null!;

        [Display(Name = "Вартість")]
        [DisplayFormat(DataFormatString = "{0:n2} ₴")]
        public decimal Cost => ProductInShop is null ? -1 : ProductInShop.Price * Count / (ProductInShop.Product.Weight is null ? 1000 : 1);

        [Display(Name = "Вага")]
        [NotMapped]
        [Range(0.0, 100.0, ErrorMessage = "Вага повинна бути в межах від 0 до 100 кілограмів")]
        [RegularExpression(@"\d+(\.\d+)?", ErrorMessage = "Введіть коректне число")]
        public decimal? Weight
        {
            get => ProductInShop is not null && ProductInShop.Product.Weight is decimal w ? w * Count : (decimal)Count / 1000;
            set
            {
                decimal res = (value ?? 0) * 1000;
                if (res > 1000000000)
                    res = 1000000000;
                if (res < -1000000000)
                    res = -1000000000;
                Count = (int)res;
            }
        }

        [Display(Name = "Вага")]
        public string FormattedWeight => ProductInShop is null ? "?" :
            ProductInShop.Product.Weight is decimal ?
                ProductInShop.Product.FormattedWeight + " × " + Count + " шт." :
                Utils.FormattedWeight(Count);
    }
}
