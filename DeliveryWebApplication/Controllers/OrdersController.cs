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
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            IEnumerable<Order> deliveryContext = _context.Orders.Include(p => p.Courier).Include(p => p.Customer).Include(p => p.Shop).Include(p => p.OrderItems).ThenInclude(item => item.ProductInShop);
            var customer = await _context.Customers.Include(t => t.Orders).FirstOrDefaultAsync(t => t.Id == Filter.CustomerId);
            var courier = await _context.Couriers.Include(t => t.Orders).FirstOrDefaultAsync(t => t.Id == Filter.CourierId);
            var shop = await _context.Shops.Alive().Include(t => t.Orders).FirstOrDefaultAsync(t => t.Id == Filter.ShopId);
            if (customer is null || !customer.Orders.Any()) Filter.CustomerId = 0;
            if (courier is null || !courier.Orders.Any()) Filter.CourierId = 0;
            if (shop is null || !shop.Orders.Any()) Filter.ShopId = 0;
            Filter = Filter;
            if (Filter.CustomerId != 0) deliveryContext = deliveryContext.Where(p => p.CustomerId == Filter.CustomerId);
            if (Filter.ShopId != 0) deliveryContext = deliveryContext.Where(p => p.ShopId == Filter.ShopId);
            deliveryContext = deliveryContext.AsEnumerable();
            if (Filter.CourierId != 0)
                deliveryContext = Filter.StatusId switch
                {
                    Order.Status.None =>
                        deliveryContext.Where(p => p.CourierId == Filter.CourierId),
                    Order.Status.Creating or Order.Status.Waiting =>
                        deliveryContext.Where(p => p.CourierId == Filter.CourierId || p.StatusId == Filter.StatusId),
                    Order.Status.Delivering or Order.Status.Refused or Order.Status.Completed =>
                        deliveryContext.Where(p => p.CourierId == Filter.CourierId && p.StatusId == Filter.StatusId),
                };
            else if(Filter.StatusId != 0)
                deliveryContext = deliveryContext.Where(p => p.StatusId == Filter.StatusId);
            ViewData["CustomerId"] = new SelectList(_context.Customers.Where(x => x.Orders.Any()).OrderBy(x => x.Name + " " + x.Phone), "Id", "NameWithPhone");
            ViewData["CourierId"] = new SelectList(_context.Couriers.Where(x => x.Orders.Any()).OrderBy(x => x.Name + " " + x.Phone), "Id", "NameWithPhone");
            ViewData["ShopId"] = new SelectList(_context.Shops.Where(x => x.Orders.Any()).OrderBy(x => x.Name + " " + x.Address), "Id", "NameWithAddress");
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
            [Display(Name = "Статус")]
            public Order.Status StatusId { get; set; } = 0;
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
                    _filter.StatusId = TempData["StatusId"] is Order.Status StatusId ? StatusId : Order.Status.None;
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
                TempData["StatusId"] = _filter.StatusId;
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "NameWithPhone");
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "NameWithAddress");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,CourierId,ShopId,DeliveryPrice,CreationTime,DeliveryTime,Address,CustomerComment,CourierComment")] Order order)
        {
            order.Address = "";
            order.CustomerComment = "";
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "NameWithPhone", order.CustomerId);
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "NameWithAddress", order.ShopId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CourierId"] = new SelectList(_context.Couriers, "Id", "Id", order.CourierId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", order.CustomerId);
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Id", order.ShopId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,CourierId,ShopId,DeliveryPrice,CreationTime,DeliveryTime,Address,CustomerComment,CourierComment")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourierId"] = new SelectList(_context.Couriers, "Id", "Id", order.CourierId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", order.CustomerId);
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Id", order.ShopId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Courier)
                .Include(o => o.Customer)
                .Include(o => o.Shop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
