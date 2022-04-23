#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeliveryWebApplication;
using System.Globalization;

namespace DeliveryWebApplication.Controllers
{
    public class OrderItemsController : Controller
    {
        private readonly DeliveryContext _context;

        public OrderItemsController(DeliveryContext context)
        {
            _context = context;
            Utils.SetCulture();
        }

        // GET: OrderItems/Create
        public IActionResult Create(int productInShopId, int shopId, string shopName, int productId, string productName, string price, string productWeight)
        {
            ViewData["ProductInShopId"] = productInShopId;
            ViewData["ShopId"] = shopId;
            ViewData["ShopName"] = shopName;
            ViewData["ProductId"] = productId;
            ViewData["ProductName"] = productName;
            ViewData["Price"] = price;
            ViewData["ProductWeight"] = productWeight;
            ViewData["OrderId"] = new SelectList(_context.Orders.Include(o => o.Customer).Where(o => o.ShopId == shopId && o.CreationTime == null), "Id", "Description", TempData["OrderId"]);
            TempData.Keep();
            return View();
        }

        // POST: OrderItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,ProductInShopId,Count,Weight")] OrderItem orderItem, int productInShopId, int shopId, string shopName, int productId, string productName, string price, string productWeight)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Orders", new { id = orderItem.OrderId });
            }
            ViewData["ProductInShopId"] = productInShopId;
            ViewData["ShopId"] = shopId;
            ViewData["ShopName"] = shopName;
            ViewData["ProductId"] = productId;
            ViewData["ProductName"] = productName;
            ViewData["Price"] = price;
            ViewData["ProductWeight"] = productWeight;
            ViewData["OrderId"] = new SelectList(_context.Orders.Include(o => o.Customer).Where(o => o.ShopId == shopId && o.CreationTime == null), "Id", "Description", TempData["OrderId"]);
            return View(orderItem);
        }

        // GET: OrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            ViewData["ProductInShopId"] = new SelectList(_context.ProductsInShops, "Id", "Id", orderItem.ProductInShopId);
            return View(orderItem);
        }

        // POST: OrderItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,ProductInShopId,Count")] OrderItem orderItem)
        {
            if (id != orderItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemExists(orderItem.Id))
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
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            ViewData["ProductInShopId"] = new SelectList(_context.ProductsInShops, "Id", "Id", orderItem.ProductInShopId);
            return View(orderItem);
        }

        // GET: OrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.ProductInShop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }
    }
}
