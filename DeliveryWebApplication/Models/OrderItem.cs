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
        [Display(Name = "Продукт")]
        public virtual ProductInShop ProductInShop { get; set; } = null!;

        [Display(Name = "Вартість")]
        [DisplayFormat(DataFormatString = "{0:n2} ₴")]
        public decimal Cost => ProductInShop is null ? -1 : ProductInShop.Price * Count / (ProductInShop.Product.Weight is null ? 1000 : 1);

        [Display(Name = "Вага")]
        [NotMapped]
        public decimal Weight
        {
            get => ProductInShop is null ? -2 : ProductInShop.Product.Weight is decimal w ? w * Count : (decimal)Count / 1000;
            set => Count = (int)value * 1000;
        }

        [Display(Name = "Вага")]
        public string FormattedWeight => ProductInShop is null ? "?" :
            ProductInShop.Product.Weight is decimal ?
                ProductInShop.Product.FormattedWeight + " × " + Count :
                Utils.FormattedWeight(Count);
    }
}
