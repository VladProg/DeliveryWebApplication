using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnotationChartController : Controller
    {
        private readonly DeliveryContext _context;

        public AnnotationChartController(DeliveryContext context)
        {
            _context = context;
            Utils.SetCulture();
        }

        [HttpGet("JsonData")]
        public JsonResult JsonData()
        {
            IEnumerable<Order> orders = _context.Shops.Include(s => s.Orders).ThenInclude(o => o.OrderItems).ThenInclude(oi => oi.ProductInShop).ThenInclude(pis => pis.Product).FirstOrDefault(s => s.Id == (int)TempData.Peek("ShopId")).Orders;
            orders = orders.Where(o => o.DeliveryTime != null);
            if (TempData.Peek("CustomerId") is int customerId and not 0)
                orders = orders.Where(o => o.CustomerId == customerId);
            if (TempData.Peek("CourierId") is int courierId and not 0)
                orders = orders.Where(o => o.CourierId == courierId);
            Dictionary<DateTime, (decimal, decimal)> money = new();
            foreach (var order in orders)
            {
                var date = order.DeliveryTime.Value.Date;
                decimal products = 0, delivery = 0;
                if (money.ContainsKey(date))
                    (products, delivery) = money[date];
                products += order.ProductsCost;
                delivery += order.DeliveryPrice.Value;
                money[date] = (products, delivery);
            }

            List<object[]> result = new() { new object[] { "Дата", "Прибуток за продукти", "Прибуток за доставку" } };
            if (!money.Any())
                return new JsonResult(result);

            var min = money.Keys.Min();
            var max = money.Keys.Max();
            var day = new TimeSpan(1, 0, 0, 0);
            for (var cur = min - day; cur <= (max + day < DateTime.Now ? max + day : DateTime.Now); cur += day)
                if (!money.ContainsKey(cur))
                    money[cur] = (0, 0);

            foreach (var date in money.Keys)
            {
                var (products, delivery) = money[date];
                result.Add(new object[] { date, products, delivery });
            }
            return new JsonResult(result);
        }
    }
}
