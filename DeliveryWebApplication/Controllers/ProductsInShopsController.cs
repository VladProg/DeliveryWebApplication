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
    public class ProductsInShopsController : MyController
    {
        private readonly DeliveryContext _context;

        public ProductsInShopsController(DeliveryContext context, UserManager<User> userManager)
            : base(userManager)
            => _context = context;

        // GET: ProductsInShops/Create
        public IActionResult Create(int productId, string productName, bool isPerKg)
        {
            if (!CheckRoles(ADMIN|SHOP)) return Forbid();
            ViewData["ProductId"] = productId;
            ViewData["ProductName"] = productName;
            ViewData["IsPerKg"] = isPerKg;
            var shops = _context.Shops.Alive().Where(s => !s.ProductsInShops.Any(pis => pis.ProductId == productId && !pis.Deleted));
            if((string)TempData.Peek("Role") == "admin")
                ViewData["ShopId"] = new SelectList(shops, "Id", "Name", TempData["ShopId"]);
            else
            {
                int id = ViewBag.UserShopId;
                ViewData["ShopId"] = new SelectList(shops.Where(s => s.Id == id), "Id", "Name");
            }
            TempData.Keep();
            return View();
        }

        // POST: ProductsInShops/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,ShopId,Price")] ProductInShop productInShop)
        {
            if (!CheckRoles(ADMIN,SHOP:productInShop.ShopId)) return Forbid();
            if (ModelState.IsValid)
            {
                _context.Add(productInShop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProductsController.Details), "Products", new { id = productInShop.ProductId });
            }
            ViewData["ProductId"] = productInShop.ProductId;
            ViewData["ProductName"] = productInShop.Product.Name;
            ViewData["IsPerKg"] = productInShop.Product.Weight is null;
            ViewData["ShopId"] = new SelectList(_context.Shops.Alive().Where(s => !s.ProductsInShops.Any(pis => pis.ProductId == productInShop.ProductId && !pis.Deleted)), "Id", "Name", TempData["ShopId"]);
            TempData.Keep();
            return View(productInShop);
        }

        // GET: ProductsInShops/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productInShop = await _context.ProductsInShops.Alive().Include(x => x.Product).Include(x => x.Shop).FirstOrDefaultAsync(x => x.Id == id);
            if (productInShop == null)
            {
                return NotFound();
            }
            if (!CheckRoles(ADMIN, SHOP: productInShop.ShopId)) return Forbid();

            ViewData["ProductName"] = productInShop.Product.Name;
            ViewData["IsPerKg"] = productInShop.Product.Weight is null;
            ViewData["ShopNameWithAddress"] = productInShop.Shop.NameWithAddress;
            return View(productInShop);
        }

        // POST: ProductsInShops/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,ShopId,Price")] ProductInShop productInShop)
        {
            if (!CheckRoles(ADMIN, SHOP: productInShop.ShopId)) return Forbid();
            if (id != productInShop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    productInShop.Id = 0;
                    _context.Add(productInShop);
                    var oldProductInShop = await _context.ProductsInShops.FindAsync(id);
                    oldProductInShop.Deleted = true;
                    _context.Update(oldProductInShop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductInShopExists(productInShop.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ProductsController.Details), "Products", new { id = productInShop.ProductId });
            }
            ViewData["ProductName"] = productInShop.Product.Name;
            ViewData["IsPerKg"] = productInShop.Product.Weight is null;
            ViewData["ShopNameWithAddress"] = productInShop.Shop.NameWithAddress;
            return View(productInShop);
        }

        // GET: ProductsInShops/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productInShop = await _context.ProductsInShops.Alive()
                .Include(p => p.Product)
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productInShop == null)
            {
                return NotFound();
            }
            if (!CheckRoles(ADMIN, SHOP: productInShop.ShopId)) return Forbid();

            ViewData["ProductName"] = productInShop.Product.Name;
            ViewData["ShopNameWithAddress"] = productInShop.Shop.NameWithAddress;
            return View(productInShop);
        }

        // POST: ProductsInShops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productInShop = await _context.ProductsInShops.FindAsync(id);
            if (!CheckRoles(ADMIN, SHOP: productInShop.ShopId)) return Forbid();
            productInShop.Deleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ProductsController.Details), "Products", new { id = productInShop.ProductId });
        }

        private bool ProductInShopExists(int id)
        {
            return _context.ProductsInShops.Any(e => e.Id == id);
        }
    }
}
