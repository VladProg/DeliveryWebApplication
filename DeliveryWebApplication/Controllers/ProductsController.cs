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
using System.Globalization;

namespace DeliveryWebApplication.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DeliveryContext _context;

        public ProductsController(DeliveryContext context)
        {
            _context = context;
            Utils.SetCulture();
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            IQueryable<Product> deliveryContext = _context.Products.Alive().Include(p => p.Category).Include(p => p.Country).Include(p => p.Trademark).Include(p => p.ProductsInShops);
            var trademark = await _context.Trademarks.Alive().Include(t=>t.Products).FirstOrDefaultAsync(t => t.Id == Filter.TrademarkId);
            var category = await _context.Categories.Alive().Include(t=>t.Products).FirstOrDefaultAsync(t => t.Id == Filter.CategoryId);
            var country = await _context.Countries.Alive().Include(t=>t.Products).FirstOrDefaultAsync(t => t.Id == Filter.CountryId);
            var shop = await _context.Shops.Alive().Include(t=>t.ProductsInShops).FirstOrDefaultAsync(t => t.Id == Filter.ShopId);
            if (trademark is null || !trademark.HasAlive) Filter.TrademarkId = 0;
            if (category is null || !category.HasAlive) Filter.CategoryId = 0;
            if (country is null || !country.HasAlive) Filter.CountryId = 0;
            if (shop is null || !shop.HasAlive) Filter.ShopId = 0;
            Filter = Filter;
            if (Filter.TrademarkId != 0) deliveryContext = deliveryContext.Where(p => p.TrademarkId == Filter.TrademarkId);
            if (Filter.CategoryId != 0) deliveryContext = deliveryContext.Where(p => p.CategoryId == Filter.CategoryId);
            if (Filter.CountryId != 0) deliveryContext = deliveryContext.Where(p => p.CountryId == Filter.CountryId);
            if (Filter.ShopId != 0) deliveryContext = deliveryContext.Where(p => p.ProductsInShops.Any(pis => pis.ShopId == Filter.ShopId && !pis.Deleted));
            ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().Where(x => x.Products.Any(y => !y.Deleted)).OrderBy(x => x.Name), "Id", "Name");
            ViewData["CountryId"] = new SelectList(_context.Countries.Alive().Where(x => x.Products.Any(y => !y.Deleted)).OrderBy(x => x.Name), "Id", "Name");
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().Where(x => x.Products.Any(y => !y.Deleted)).OrderBy(x => x.Name), "Id", "Name");
            ViewData["ShopId"] = new SelectList(_context.Shops.Alive().Where(x => x.ProductsInShops.Any(y => !y.Deleted)).OrderBy(x => x.Name + " " + x.Address), "Id", "NameWithAddress");
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

        private FilterClass _filter = null;

        public FilterClass Filter
        {
            get
            {
                if (_filter is null)
                {
                    _filter = new();
                    _filter.CategoryId = TempData["CategoryId"] is int CategoryId ? CategoryId : 0;
                    _filter.TrademarkId = TempData["TrademarkId"] is int TrademarkId ? TrademarkId : 0;
                    _filter.CountryId = TempData["CountryId"] is int CountryId ? CountryId : 0;
                    _filter.ShopId = TempData["ShopId"] is int ShopId ? ShopId : 0;
                }
                TempData.Keep();
                return _filter;
            }
            set
            {
                _filter = value;
                TempData["CategoryId"] = _filter.CategoryId;
                TempData["TrademarkId"] = _filter.TrademarkId;
                TempData["CountryId"] = _filter.CountryId;
                TempData["ShopId"] = _filter.ShopId;
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
            public Product Product { get; set; }
            public OrderItem OrderItem { get; set; }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Alive()
                .Include(p => p.Category)
                .Include(p => p.Country)
                .Include(p => p.Trademark)
                .Include(p => p.ProductsInShops)
                .ThenInclude(p => p.Shop)
                .ThenInclude(p => p.Orders)
                .ThenInclude(p => p.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(new DetailsInfo { Product = product });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, DetailsInfo info)
        {
            var product = await _context.Products.FindAsync(id);
            if(product.Weight is null)
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

            int customerId = (int)TempData.Peek("CustomerId");
            int productInShopId = info.OrderItem.ProductInShopId;
            int shopId = (await _context.ProductsInShops.FindAsync(productInShopId)).ShopId;
            var order = _context.Orders.Include(o=>o.OrderItems).FirstOrDefault(o => o.CustomerId == customerId && o.ShopId == shopId && o.CreationTime == null);
            if (order == null)
            {
                order = new Order();
                order.CustomerId = customerId;
                order.ShopId = shopId;
                order.Address = "";
                order.CustomerComment = "";
                try
                {
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }

            var orderItem = order.OrderItems.Where(oi => oi.OrderId == order.Id && oi.ProductInShopId == productInShopId).FirstOrDefault();
            if (orderItem == null)
            {
                orderItem = new OrderItem();
                orderItem.OrderId = order.Id;
                orderItem.ProductInShopId = productInShopId;
                _context.Add(orderItem);
            }
            orderItem.Count = info.OrderItem.Count;
            if(orderItem.Count == 0)
                _context.Remove(orderItem);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            if ((await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == order.Id)).OrderItems.Count == 0)
                try
                {
                    _context.Remove(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }

            return RedirectToAction(nameof(Details), new { id });
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
            ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.TrademarkId);
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInfo info)
        {
            if (ModelState.IsValid &&
                _context.Products.Alive().Any(p => p.Name == info.Product.Name &&
                                                   p.Weight == info.Product.Weight &&
                                                   p.TrademarkId == info.Product.TrademarkId &&
                                                   p.CategoryId == info.Product.CategoryId &&
                                                   p.CountryId == info.Product.CountryId))
                ModelState.AddModelError("", "Такий товар вже існує");
            if (ModelState.IsValid)
            {
                _context.Add(info.Product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = info.Product.Id });
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name");
            ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name");
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name");
            return View(info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTrademark(CreateInfo info)
        {
            if (_context.Trademarks.Alive().Any(t => t.Name == info.Trademark.Name))
                ModelState.AddModelError("Trademark.Name", "Така торгова марка вже існує");
            if (ModelState.IsValid)
            {
                var entity = _context.Add(info.Trademark).Entity;
                await _context.SaveChangesAsync();
                ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.CategoryId);
                ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.CountryId);
                ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name", entity.Id);
                return View(nameof(Create), info);
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.TrademarkId);
            return View(nameof(Create), info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateInfo info)
        {
            if (_context.Categories.Alive().Any(c => c.Name == info.Category.Name))
                ModelState.AddModelError("Category.Name", "Така категорія вже існує");
            if (ModelState.IsValid)
            {
                var entity = _context.Add(info.Category).Entity;
                await _context.SaveChangesAsync();
                ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name", entity.Id);
                ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.CountryId);
                ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.TrademarkId);
                return View(nameof(Create), info);
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name", Filter.TrademarkId);
            return View(nameof(Create), info);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.AliveFindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name", product.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name", product.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name", product.TrademarkId);
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

            if (ModelState.IsValid &&
                _context.Products.Alive().Any(p => p.Name == info.Product.Name &&
                                                   p.Weight == info.Product.Weight &&
                                                   p.TrademarkId == info.Product.TrademarkId &&
                                                   p.CategoryId == info.Product.CategoryId &&
                                                   p.CountryId == info.Product.CountryId &&
                                                   p.Id != id))
                ModelState.AddModelError("", "Такий товар вже існує");
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
            ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name", info.Product.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name", info.Product.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name", info.Product.TrademarkId);
            return View(info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTrademark(int id, CreateInfo info)
        {
            var product = await _context.Products.AliveFindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            info.Product = product;

            if (_context.Trademarks.Alive().Any(t => t.Name == info.Trademark.Name))
                ModelState.AddModelError("Trademark.Name", "Така торгова марка вже існує");
            if (ModelState.IsValid)
            {
                var entity = _context.Add(info.Trademark).Entity;
                await _context.SaveChangesAsync();
                product.TrademarkId = entity.Id;
                ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name");
                ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name");
                ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name");
                return View(nameof(Edit), info);
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name", info.Product.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name", info.Product.CountryId);
            ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name", info.Product.TrademarkId);
            return View(nameof(Edit), info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, CreateInfo info)
        {
            var product = await _context.Products.AliveFindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            info.Product = product;

            if (_context.Categories.Alive().Any(c => c.Name == info.Category.Name))
                ModelState.AddModelError("Category.Name", "Така категорія вже існує");
            if (ModelState.IsValid)
            {
                var entity = _context.Add(info.Category).Entity;
                await _context.SaveChangesAsync();
                product.CategoryId = entity.Id;
                ViewData["CategoryId"] = new SelectList(_context.Categories.Alive().OrderBy(x => x.Name), "Id", "Name");
                ViewData["CountryId"] = new SelectList(_context.Countries.Alive().OrderBy(x => x.Name), "Id", "Name");
                ViewData["TrademarkId"] = new SelectList(_context.Trademarks.Alive().OrderBy(x => x.Name), "Id", "Name");
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

            var product = await _context.Products.Alive()
                .Include(p => p.Category)
                .Include(p => p.Country)
                .Include(p => p.Trademark)
                .Include(p => p.ProductsInShops)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null || product.HasAlive)
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
            product.Deleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Import(IFormFile fileExcel)
        {
            if (!ModelState.IsValid)
                return View(nameof(Index));
            try
            {
                using var reader = new ExcelReader(fileExcel);
                if (!reader.Rows().Any())
                    throw new InvalidDataException("Аркуш порожній");
                int nameIndex = -1, weightIndex = -1, trademarkIndex = -1, categoryIndex = -1, countryIndex = -1;
                List<(int Index, Shop Shop)> shops = new();

                foreach (var cell in reader.Rows().First().Cells())
                {
                    switch (cell.GetString().ToLower().Trim().Split()[0])
                    {
                        case "назва":
                            nameIndex = cell.WorksheetColumn().ColumnNumber();
                            break;
                        case "вага":
                            weightIndex = cell.WorksheetColumn().ColumnNumber();
                            break;
                        case "торгова":
                            trademarkIndex = cell.WorksheetColumn().ColumnNumber();
                            break;
                        case "категорія":
                            categoryIndex = cell.WorksheetColumn().ColumnNumber();
                            break;
                        case "країна":
                            countryIndex = cell.WorksheetColumn().ColumnNumber();
                            break;
                        default:
                            string str = cell.GetString().Trim();
                            if (str.Length == 0)
                                break;
                            int pos1 = str.IndexOf('(');
                            int pos2 = str.LastIndexOf(')');
                            if (pos1 == -1 || pos2 == -1 || pos1 > pos2)
                                throw new InvalidDataException("Некоректний стовпчик: " + str);
                            Shop shop = new() { Name = str[..pos1].Trim(), Address = str[(pos1 + 1)..pos2].Trim(), Phone = "?", Site = "?" };
                            shop = _context.FindOrAdd(_context.Shops, s => s.Name == shop.Name && s.Address == shop.Address, shop);
                            shops.Add((cell.WorksheetColumn().ColumnNumber(), shop));
                            break;
                    }
                }
                if (nameIndex == -1)
                    throw new InvalidDataException("Немає стовпчика \"Назва\"");

                foreach (var row in reader.Rows().Skip(1))
                {
                    string name = row.Cell(nameIndex).GetString();
                    decimal? weight = weightIndex == -1 || row.Cell(weightIndex).IsEmpty() ? 0 : (decimal)row.Cell(weightIndex).GetDouble();
                    string trademark = trademarkIndex == -1 ? "" : row.Cell(trademarkIndex).GetString();
                    string category = categoryIndex == -1 ? "" : row.Cell(categoryIndex).GetString();
                    string country = countryIndex == -1 ? "" : row.Cell(countryIndex).GetString();
                    if (name == "") throw new InvalidDataException("У кожного продукта має бути назва");
                    if (weight <= 0) weight = null;
                    if (trademark == "") trademark = "—";
                    if (category == "") category = "—";
                    if (country == "") country = "—";
                    Product product = new()
                    {
                        Name = name,
                        Weight = weight,
                        Trademark = _context.FindOrAdd(_context.Trademarks, t => t.Name == trademark, new() { Name = trademark }),
                        Category = _context.FindOrAdd(_context.Categories, t => t.Name == category, new() { Name = category }),
                        Country = _context.FindOrAdd(_context.Countries, t => t.Name == country, new() { Name = country })
                    };
                    product = _context.FindOrAdd(_context.Products,
                                                 p => p.Name == product.Name &&
                                                      p.Weight == product.Weight &&
                                                      p.Trademark == product.Trademark &&
                                                      p.Category == product.Category &&
                                                      p.Country == product.Country,
                                                 product);
                    foreach (var (index, shop) in shops)
                    {
                        decimal price = row.Cell(index).IsEmpty() ? 0 : (decimal)row.Cell(index).GetDouble();
                        var productInShop = _context.ProductsInShops.Alive().FirstOrDefault(pis => pis.ProductId == product.Id && pis.ShopId == shop.Id);
                        if (productInShop is not null)
                            productInShop.Deleted = true;
                        if (price > 0)
                            _context.ProductsInShops.Add(new() { Product = product, Shop = shop, Price = price });
                        _context.SaveChanges();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                Index().Wait();
                return View(nameof(Index));
            }
        }

        public ActionResult Export()
        {
            using ExcelWriter writer = new();
            var shops = _context.Shops.Alive().ToList();
            writer.AddRow(new string[] { "Назва", "Вага (кг)", " Торгова марка", "Категорія", "Країна виробництва" },
                          from shop in shops select shop.NameWithAddress);
            Index().Wait();
            foreach (var product in Filter.Products.OrderBy(p => p.Name))
            {
                var productsInShops = product.ProductsInShops.Alive().ToList();
                writer.AddRow(new object[] { product.Name, product.Weight, product.Trademark.Name, product.Category.Name, product.Country.Name },
                              from shop in shops select (object)productsInShops.FirstOrDefault(pis => pis.ShopId == shop.Id)?.Price);
            }
            string name = "Продукти";
            if (Filter.TrademarkId != 0) name += ", торгова марка '" + _context.Trademarks.Find(Filter.TrademarkId).Name + "'";
            if (Filter.CategoryId != 0) name += ", категорія '" + _context.Categories.Find(Filter.CategoryId).Name + "'";
            if (Filter.CountryId != 0) name += ", країна виробництва '" + _context.Countries.Find(Filter.CountryId).Name + "'";
            if (Filter.ShopId != 0) name += ", магазин '" + _context.Shops.Find(Filter.ShopId).NameWithAddress + "'";
            return writer.Save(name);
        }
    }
}
