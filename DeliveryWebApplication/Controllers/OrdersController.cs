#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeliveryWebApplication;

namespace DeliveryWebApplication.Controllers
{
    public class OrdersController : Controller
    {
        private readonly DeliveryContext _context;

        public OrdersController(DeliveryContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var deliveryContext = _context.Orders.Include(o => o.Courier).Include(o => o.Customer).Include(o => o.Shop);
            return View(await deliveryContext.ToListAsync());
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
            ViewData["CourierId"] = new SelectList(_context.Couriers, "Id", "Id");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id");
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Id");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,CourierId,ShopId,DeliveryPrice,CreationTime,DeliveryTime,Address,CustomerComment,CourierComment")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourierId"] = new SelectList(_context.Couriers, "Id", "Id", order.CourierId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", order.CustomerId);
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Id", order.ShopId);
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
