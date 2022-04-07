#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeliveryWebApplication;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DeliveryWebApplication.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DeliveryContext _context;

        public ProductsController(DeliveryContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            IQueryable<Product> deliveryContext = _context.Products.Include(p => p.Category).Include(p => p.Country).Include(p => p.Trademark).Include(p => p.ProductsInShops);
            if (Filter.TrademarkId != 0) deliveryContext = deliveryContext.Where(p => p.TrademarkId == Filter.TrademarkId);
            if (Filter.CategoryId != 0) deliveryContext = deliveryContext.Where(p => p.CategoryId == Filter.CategoryId);
            if (Filter.CountryId != 0) deliveryContext = deliveryContext.Where(p => p.CountryId == Filter.CountryId);
            if (Filter.ShopId != 0) deliveryContext = deliveryContext.Where(p => p.ProductsInShops.Any(pis => pis.ShopId == Filter.ShopId));
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name");
            ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name");
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name");
            ViewData["ShopId"] = new SelectList(_context.Shops.OrderBy(x => x.Name + " " + x.Address), "Id", "NameWithAddress");
            Filter.Products = await deliveryContext.ToListAsync();
            return View(Filter);
        }

        public class FilterClass
        {
            public FilterClass() { }
            public List<Product> Products;
            [Display(Name = "Категорія")]
            public int CategoryId { get; set; } = 0;
            [Display(Name = "Торгова марка")]
            public int TrademarkId { get; set; } = 0;
            [Display(Name = "Країна виробництва")]
            public int CountryId { get; set; } = 0;
            [Display(Name = "Магазин")]
            public int ShopId { get; set; } = 0;
        }

        public static FilterClass Filter=new();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FilterClass model)
        {
            Filter = model;
            return await Index();
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Country)
                .Include(p => p.Trademark)
                .Include(p => p.ProductsInShops)
                .ThenInclude(p => p.Shop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public class CreateInfo
        {
            public Product Product { get; set; }
            public Trademark Trademark { get; set; }
            public Category Category { get; set; }
        }


        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name", Filter.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name", Filter.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name", Filter.TrademarkId);
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInfo info)
        {
            if (ModelState.IsValid)
            {
                _context.Add(info.Product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name");
            ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name");
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name");
            return View(info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTrademark(CreateInfo info)
        {
            if (ModelState.IsValid)
            {
                var entity = _context.Add(info.Trademark).Entity;
                await _context.SaveChangesAsync();
                ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name", Filter.CategoryId);
                ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name", Filter.CountryId);
                ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name", entity.Id);
                return View(nameof(Create), info);
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name");
            ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name");
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name");
            return View(nameof(Create), info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateInfo info)
        {
            if (ModelState.IsValid)
            {
                var entity = _context.Add(info.Category).Entity;
                await _context.SaveChangesAsync();
                ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name", entity.Id);
                ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name", Filter.CountryId);
                ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name", Filter.TrademarkId);
                return View(nameof(Create), info);
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name");
            ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name");
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name");
            return View(nameof(Create), info);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name", product.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name", product.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name", product.TrademarkId);
            return View(new CreateInfo { Product = product });
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateInfo info)
        {
            if (id != info.Product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(info.Product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(info.Product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name", info.Product.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name", info.Product.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name", info.Product.TrademarkId);
            return View(info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTrademark(int id, CreateInfo info)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            info.Product = product;

            if (ModelState.IsValid)
            {
                var entity = _context.Add(info.Trademark).Entity;
                await _context.SaveChangesAsync();
                product.TrademarkId = entity.Id;
                ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name");
                ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name");
                ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name");
                return View(nameof(Edit), info);
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name", info.Product.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name", info.Product.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name", info.Product.TrademarkId);
            return View(nameof(Edit), info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, CreateInfo info)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            info.Product = product;

            if (ModelState.IsValid)
            {
                var entity = _context.Add(info.Category).Entity;
                await _context.SaveChangesAsync();
                product.CategoryId = entity.Id;
                ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name");
                ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name");
                ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name");
                return View(nameof(Edit), info);
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(x => x.Name), "Id", "Name", info.Product.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.OrderBy(x => x.Name), "Id", "Name", info.Product.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.OrderBy(x => x.Name), "Id", "Name", info.Product.TrademarkId);
            return View(nameof(Edit), info);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Country)
                .Include(p => p.Trademark)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
