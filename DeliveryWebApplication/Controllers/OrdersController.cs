#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeliveryWebApplication;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication.Controllers
{
    public class OrdersController : Controller
    {
        private readonly DeliveryContext _context;

        public OrdersController(DeliveryContext context)
        {
            _context = context;
            Utils.SetCulture();
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            if (!Filter.Creating && !Filter.Waiting && !Filter.Delivering && !Filter.Refused && !Filter.Completed)
                Filter.Creating = Filter.Waiting = Filter.Delivering = Filter.Refused = Filter.Completed = true;
            IEnumerable<Order> deliveryContext = _context.Orders.Include(p => p.Courier).Include(p => p.Customer).Include(p => p.Shop).Include(p => p.OrderItems).ThenInclude(item => item.ProductInShop).ThenInclude(pis => pis.Product);
            var customer = await _context.Customers.Include(t => t.Orders).FirstOrDefaultAsync(t => t.Id == Filter.CustomerId);
            var courier = await _context.Couriers.Include(t => t.Orders).FirstOrDefaultAsync(t => t.Id == Filter.CourierId);
            var shop = await _context.Shops.Include(t => t.Orders).Include(t => t.ProductsInShops).FirstOrDefaultAsync(t => t.Id == Filter.ShopId);
            if (customer is null) Filter.CustomerId = 0;
            if (courier is null) Filter.CourierId = 0;
            if (shop is null || !shop.Orders.Any() && !shop.ProductsInShops.Any(y => !y.Deleted)) Filter.ShopId = 0;
            Filter = Filter;
            if (Filter.CustomerId != 0) deliveryContext = deliveryContext.Where(p => p.CustomerId == Filter.CustomerId);
            if (Filter.ShopId != 0) deliveryContext = deliveryContext.Where(p => p.ShopId == Filter.ShopId);
            if (Filter.CourierId != 0) deliveryContext = deliveryContext.Where(o => o.CourierId is null || o.CourierId == Filter.CourierId || o.CourierComment is not null);
            deliveryContext = deliveryContext.AsEnumerable();
            if (!Filter.Creating) deliveryContext = deliveryContext.Where(o => o.StatusId != Order.Status.Creating);
            if (!Filter.Waiting) deliveryContext = deliveryContext.Where(o => o.StatusId != Order.Status.Waiting);
            if (!Filter.Delivering) deliveryContext = deliveryContext.Where(o => o.StatusId != Order.Status.Delivering);
            if (!Filter.Refused) deliveryContext = deliveryContext.Where(o => o.StatusId != Order.Status.Refused);
            if (!Filter.Completed) deliveryContext = deliveryContext.Where(o => o.StatusId != Order.Status.Completed);
            ViewData["CustomerId"] = new SelectList(_context.Customers.OrderBy(x => x.Phone + " " + x.Name), "Id", "NameWithPhone");
            ViewData["CourierId"] = new SelectList(_context.Couriers.OrderBy(x => x.Phone + " " + x.Name), "Id", "NameWithPhone");
            ViewData["ShopId"] = new SelectList(_context.Shops.Where(x => x.Orders.Any() || x.ProductsInShops.Any(y => !y.Deleted)).OrderBy(x=>x.Deleted).ThenBy(x => x.Name + " " + x.Address), "Id", "NameWithAddress");
            ViewData["StatusId"] = new SelectList(Order.STATUS_NAMES.Select((Name, Id) => new { Name, Id }), "Id", "Name");
            Filter.Orders = deliveryContext.ToList();
            TempData["Back"] = "Orders";
            return View(Filter);
        }

        public class FilterClass
        {
            public FilterClass() { }
            public List<Order> Orders;
            [Display(Name = "Клієнт")]
            public int CustomerId { get; set; } = 0;
            [Display(Name = "Кур'єр")]
            public int CourierId { get; set; } = 0;
            [Display(Name = "Магазин")]
            public int ShopId { get; set; } = 0;

            public bool Creating { get; set; } = true;
            public bool Waiting { get; set; } = true;
            public bool Delivering { get; set; } = true;
            public bool Refused { get; set; } = true;
            public bool Completed { get; set; } = true;
        }

        private FilterClass _filter = null;

        public FilterClass Filter
        {
            get
            {
                if (_filter is null)
                {
                    _filter = new();
                    _filter.CustomerId = TempData["CustomerId"] is int CustomerId ? CustomerId : 0;
                    _filter.CourierId = TempData["CourierId"] is int CourierId ? CourierId : 0;
                    _filter.ShopId = TempData["ShopId"] is int ShopId ? ShopId : 0;
                    _filter.Creating = TempData["Creating"] as bool? ?? true;
                    _filter.Waiting = TempData["Waiting"] as bool? ?? true;
                    _filter.Delivering = TempData["Delivering"] as bool? ?? true;
                    _filter.Refused = TempData["Refused"] as bool? ?? true;
                    _filter.Completed = TempData["Completed"] as bool? ?? true;
                }
                TempData.Keep();
                return _filter;
            }
            set
            {
                _filter = value;
                TempData["CustomerId"] = _filter.CustomerId;
                TempData["CourierId"] = _filter.CourierId;
                TempData["ShopId"] = _filter.ShopId;
                TempData["Creating"] = _filter.Creating;
                TempData["Waiting"] = _filter.Waiting;
                TempData["Delivering"] = _filter.Delivering;
                TempData["Refused"] = _filter.Refused;
                TempData["Completed"] = _filter.Completed;
                TempData.Keep();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FilterClass model)
        {
            Filter = model;
            return await Index();
        }

        public class DetailsInfo
        {
            public Order Order { get; set; }
            public OrderItem OrderItem { get; set; }
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Courier)
                .Include(o => o.Customer)
                .Include(o => o.Shop)
                .Include(o => o.OrderItems)
                .ThenInclude(item => item.ProductInShop)
                .ThenInclude(pis => pis.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(new DetailsInfo { Order = order });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, DetailsInfo info)
        {
            var product = _context.OrderItems.Include(oi => oi.ProductInShop).ThenInclude(pis => pis.Product).FirstOrDefault(oi => oi.Id == info.OrderItem.Id).ProductInShop.Product;
            if (product.Weight is null)
            {
                if (info.OrderItem.Weight > 100)
                    info.OrderItem.Weight = 100;
            }
            else
            {
                if (info.OrderItem.Count > 100)
                    info.OrderItem.Count = 100;
            }
            if (info.OrderItem.Count < 0)
                info.OrderItem.Count = 0;

            if (!ModelState.IsValid)
                return await Details(id);

            var order = await _context.Orders.FindAsync(id);
            var orderItem = await _context.OrderItems.FindAsync(info.OrderItem.Id);
            orderItem.Count = info.OrderItem.Count;
            if (orderItem.Count == 0)
                _context.Remove(orderItem);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            if ((await _context.Orders.Include(o=>o.OrderItems).FirstOrDefaultAsync(o=>o.Id==order.Id)).OrderItems.Count == 0)
                try
                {
                    _context.Remove(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.Include(o => o.Shop).Include(o => o.OrderItems).ThenInclude(oi => oi.ProductInShop).ThenInclude(pis => pis.Product).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            var prices = from o in await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.ProductInShop).ThenInclude(pis => pis.Product).ToListAsync()
                         where o.CreationTime != null && (o.CourierId == null || o.CourierComment != null)
                         let price=(decimal)o.DeliveryPrice/o.Weight
                         orderby price
                         select price;
            decimal median = prices.Any() ?
                             (prices.ElementAt(prices.Count() / 2) + prices.ElementAt((prices.Count() - 1) / 2)) / 2 * order.Weight :
                             0;
            median = decimal.Round(median);
            if (median < Order.MINIMAL_DELIVERY_PRICE)
                median = Order.MINIMAL_DELIVERY_PRICE;
            if (median > Order.MAXIMAL_DELIVERY_PRICE)
                median = Order.MAXIMAL_DELIVERY_PRICE;
            ViewData["Median"] = median;

            return View(order);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id, [Bind("Id,CustomerId,CourierId,ShopId,DeliveryPrice,CreationTime,DeliveryTime,Address,CustomerComment,CourierComment")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (order.CustomerComment is null)
                order.CustomerComment = "";
            if (order.DeliveryPrice is null)
                ModelState.AddModelError("DeliveryPrice", "Введіть ціну доставки");

            var order1 = await _context.Orders.Include(o => o.Shop).Include(o => o.OrderItems).ThenInclude(oi => oi.ProductInShop).ThenInclude(pis => pis.Product).FirstOrDefaultAsync(o => o.Id == id);
            order1.DeliveryPrice = order.DeliveryPrice;
            order1.Address = order.Address;
            order1.CustomerComment = order.CustomerComment;
            if (order1.CourierComment is not null && !order1.CourierComment.EndsWith(" (після цього коментаря замовлення було змінене)"))
                order1.CourierComment += " (після цього коментаря замовлення було змінене)";
            if (ModelState.IsValid)
            {
                order1.CreationTime = DateTime.Now;
                try
                {
                    _context.Update(order1);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order1.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var prices = from o in await _context.Orders.ToListAsync()
                         where o.CreationTime != null && (o.CourierId == null || o.CourierComment != null)
                         let price = (decimal)o.DeliveryPrice / o.Weight
                         orderby price
                         select price;
            decimal median = prices.Any() ?
                             (prices.ElementAt(prices.Count() / 2) + prices.ElementAt((prices.Count() - 1) / 2)) / 2 * order.Weight :
                             0;
            if (median < Order.MINIMAL_DELIVERY_PRICE)
                median = Order.MINIMAL_DELIVERY_PRICE;
            if (median > Order.MAXIMAL_DELIVERY_PRICE)
                median = Order.MAXIMAL_DELIVERY_PRICE;
            ViewData["Median"] = median;

            return View(order1);
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order is null)
            {
                return NotFound();
            }
            order.CreationTime = null;

            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        public async Task<IActionResult> Deliver(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            var courierId = (int?)TempData.Peek("CourierId") ?? 0;
            if (order is null || courierId == 0)
            {
                return NotFound();
            }
            order.CourierId = courierId;
            order.CourierComment = null;

            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: Orders/Create
        public async Task<IActionResult> Refuse(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.Include(o => o.Shop).Include(o => o.OrderItems).ThenInclude(oi => oi.ProductInShop).ThenInclude(pis => pis.Product).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refuse(int id, [Bind("Id,CustomerId,CourierId,ShopId,DeliveryPrice,CreationTime,DeliveryTime,Address,CustomerComment,CourierComment")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            var order1 = await _context.Orders.Include(o => o.Shop).Include(o => o.OrderItems).ThenInclude(oi => oi.ProductInShop).ThenInclude(pis => pis.Product).FirstOrDefaultAsync(o => o.Id == id);
            order1.CourierComment = order.CourierComment;
            if (string.IsNullOrWhiteSpace(order.CourierComment))
                ModelState.AddModelError("CourierComment", "Залиште коментар");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order1);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order1.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return View(order1);
        }

        public async Task<IActionResult> Done(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            order.DeliveryTime = DateTime.Now;

            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
