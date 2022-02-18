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
    public class ProductsInShopsController : Controller
    {
        private readonly DeliveryContext _context;

        public ProductsInShopsController(DeliveryContext context)
        {
            _context = context;
        }

        // GET: ProductsInShops
        public async Task<IActionResult> Index()
        {
            var deliveryContext = _context.ProductsInShops.Include(p => p.Product).Include(p => p.Shop);
            return View(await deliveryContext.ToListAsync());
        }

        // GET: ProductsInShops/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productInShop = await _context.ProductsInShops
                .Include(p => p.Product)
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productInShop == null)
            {
                return NotFound();
            }

            return View(productInShop);
        }

        // GET: ProductsInShops/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id");
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Id");
            return View();
        }

        // POST: ProductsInShops/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,ShopId,Price")] ProductInShop productInShop)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productInShop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", productInShop.ProductId);
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Id", productInShop.ShopId);
            return View(productInShop);
        }

        // GET: ProductsInShops/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productInShop = await _context.ProductsInShops.FindAsync(id);
            if (productInShop == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", productInShop.ProductId);
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Id", productInShop.ShopId);
            return View(productInShop);
        }

        // POST: ProductsInShops/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,ShopId,Price")] ProductInShop productInShop)
        {
            if (id != productInShop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productInShop);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", productInShop.ProductId);
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Id", productInShop.ShopId);
            return View(productInShop);
        }

        // GET: ProductsInShops/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productInShop = await _context.ProductsInShops
                .Include(p => p.Product)
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productInShop == null)
            {
                return NotFound();
            }

            return View(productInShop);
        }

        // POST: ProductsInShops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productInShop = await _context.ProductsInShops.FindAsync(id);
            _context.ProductsInShops.Remove(productInShop);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductInShopExists(int id)
        {
            return _context.ProductsInShops.Any(e => e.Id == id);
        }
    }
}
