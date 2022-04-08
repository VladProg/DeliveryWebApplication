using DeliveryWebApplication.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DeliveryWebApplication
{
    public partial class Product
    {
        public Product()
        {
            ProductsInShops = new HashSet<ProductInShop>();
        }

        public int Id { get; set; }
        [Display(Name = "Назва")]
        [Required(ErrorMessage = "Введіть назву")]
        [MaxLength(100, ErrorMessage = "Назва не може бути довша, ніж 100 символів")]
        public string Name { get; set; } = null!;
        [Display(Name = "Вага")]
        [Range(1, int.MaxValue, ErrorMessage = "Вага повинна бути або відсутня, або додатна")]
        public int? Weight { get; set; }
        [Display(Name = "Торгова марка")]
        [Required(ErrorMessage = "Оберіть торгову марку")]
        public int TrademarkId { get; set; }
        [Display(Name = "Категорія")]
        [Required(ErrorMessage = "Оберіть категорію")]
        public int CategoryId { get; set; }
        [Display(Name = "Країна виробництва")]
        [Required(ErrorMessage = "Оберіть країну виробництва")]
        public int CountryId { get; set; }
        public bool Deleted { get; set; } = false;

        [Display(Name = "Категорія")]
        public virtual Category Category { get; set; } = null!;
        [Display(Name = "Країна виробництва")]
        public virtual Country Country { get; set; } = null!;
        [Display(Name = "Торгова марка")]
        public virtual Trademark Trademark { get; set; } = null!;
        public virtual ICollection<ProductInShop> ProductsInShops { get; set; }

        [Display(Name = "Вага")]
        public string WeightOrEmpty => Weight is null ? "—" :
                                                        Weight < 1000 ? Weight.ToString() + " г" :
                                                                        (Weight / 1000.0)?.ToString("0.###") + " кг";

        [Display(Name = "Ціна")]
        public string this[int shopId]
        {
            get
            {
                IEnumerable<ProductInShop> productsInShops = ProductsInShops;
                if (shopId != 0)
                    productsInShops = productsInShops.Where(pis => pis.ShopId == shopId);
                if (!ProductsInShops.Any())
                    return "—";
                var min = productsInShops.Min(pis => pis.Price);
                var max = productsInShops.Max(pis => pis.Price);
                if (Weight is null)
                    if (min == max)
                        return (max * 1000).ToString("0.00") + " ₴/кг";
                    else
                        return (min * 1000).ToString("0.00") + " — " + (max * 1000).ToString("0.00") + " ₴/кг";
                else
                    if (min == max)
                        return max.ToString("0.00") + " ₴";
                    else
                        return min.ToString("0.00") + " — " + max.ToString("0.00") + " ₴";
            }
        }

        [Display(Name = "Ціна")]
        public string Prices
        {
            get
            {
                IEnumerable<ProductInShop> productsInShops = ProductsInShops;
                if (!ProductsInShops.Any())
                    return "—";
                var min = productsInShops.Min(pis => pis.Price);
                var max = productsInShops.Max(pis => pis.Price);
                if (Weight is null)
                    if (min == max)
                        return (max * 1000).ToString("0.00") + " ₴/кг";
                    else
                        return (min * 1000).ToString("0.00") + " — " + (max * 1000).ToString("0.00") + " ₴/кг";
                else
                    if (min == max)
                        return max.ToString("0.00") + " ₴";
                    else
                        return min.ToString("0.00") + " — " + max.ToString("0.00") + " ₴";
            }
        }
    }
}
