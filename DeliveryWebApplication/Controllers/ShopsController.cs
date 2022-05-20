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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DeliveryWebApplication.Controllers
{
    public class ShopsController : MyController
    {
        private readonly DeliveryContext _context;

        public ShopsController(DeliveryContext context, UserManager<User> userManager)
            : base(userManager)
            => _context = context;

        // GET: Shops
        public async Task<IActionResult> Index(string back = "")
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            ViewData["Back"] = back;
            return View(await _context.Shops.Alive().Include(s => s.ProductsInShops).ToListAsync());
        }

        // GET: Shops/Create
        public IActionResult Create(string back = "")
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            ViewData["Back"] = back;
            return View();
        }

        // POST: Shops/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Phone,Site")] Shop shop, string back = "")
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            ViewData["Back"] = back;
            if (_context.Shops.Alive().Any(s => s.Name == shop.Name && s.Address == shop.Address))
                ModelState.AddModelError("", "Такий магазин вже існує");
            if (ModelState.IsValid)
            {
                _context.Add(shop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { back });
            }
            return View(shop);
        }

        // GET: Shops/Edit/5
        public async Task<IActionResult> Edit(int? id, string back = "")
        {
            if (!CheckRoles(ADMIN,SHOP:id)) return Forbid();
            ViewData["Back"] = back;
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops.AliveFindAsync(id);
            if (shop == null)
            {
                return NotFound();
            }
            return View(shop);
        }

        // POST: Shops/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Phone,Site")] Shop shop, string back = "")
        {
            if (!CheckRoles(ADMIN, SHOP: id)) return Forbid();
            ViewData["Back"] = back;
            if (id != shop.Id)
            {
                return NotFound();
            }

            if (_context.Shops.Alive().Any(s => s.Name == shop.Name && s.Address == shop.Address && s.Id != shop.Id))
                ModelState.AddModelError("", "Такий магазин вже існує");
            var shop1 = _context.Shops.AsNoTracking().FirstOrDefault(s => s.Id == shop.Id);
            shop1.Address = shop.Address;
            shop1.Name = shop.Name;
            shop1.Phone = shop.Phone;
            shop1.Site = shop.Site;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopExists(shop.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), (string)TempData.Peek("Role") == "admin" ? "Shops" : "Home", new { back });
            }
            return View(shop);
        }

        // GET: Shops/Delete/5
        public async Task<IActionResult> Delete(int? id, string back = "")
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            ViewData["Back"] = back;
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops.Alive()
                .Include(t => t.ProductsInShops)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null || shop.HasAlive)
            {
                return NotFound();
            }

            return View(shop);
        }

        // POST: Shops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string back = "")
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            ViewData["Back"] = back;
            var shop = await _context.Shops.FindAsync(id);
            shop.Deleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { back });
        }

        private bool ShopExists(int id)
        {
            return _context.Shops.Any(e => e.Id == id);
        }
    }
}
