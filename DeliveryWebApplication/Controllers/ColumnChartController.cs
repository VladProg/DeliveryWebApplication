using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColumnChartController : Controller
    {
        private readonly DeliveryContext _context;

        public ColumnChartController(DeliveryContext context)
        {
            _context = context;
        }

        [HttpGet("JsonData")]
        public JsonResult JsonData()
        {
            var shops = _context.Shops.Include(s => s.Orders).ThenInclude(o => o.OrderItems).ThenInclude(oi => oi.ProductInShop).ThenInclude(pis => pis.Product);
            List<object[]> result = new() { new object[] { "Магазин", "Прибуток за продукти (₴)", "Прибуток за доставку (₴)" } };
            foreach (var shop in shops)
            {
                var orders = shop.Orders.Where(o => o.DeliveryTime != null);
                if (TempData.Peek("CustomerId") is int customerId and not 0)
                    orders = orders.Where(o => o.CustomerId == customerId);
                if (TempData.Peek("CourierId") is int courierId and not 0)
                    orders = orders.Where(o => o.CourierId == courierId);
                if (orders.Any())
                    result.Add(new object[] { shop.NameWithAddress, orders.Sum(o=>o.ProductsCost), orders.Sum(o=>o.DeliveryPrice) });
            }
            return new JsonResult(result);
        }
    }
}
