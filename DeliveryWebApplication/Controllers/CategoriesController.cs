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
    public class CategoriesController : MyController
    {
        private readonly DeliveryContext _context;

        public CategoriesController(DeliveryContext context, UserManager<User> userManager)
            : base(userManager)
            => _context = context;

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            if (!CheckRoles(ADMIN|SHOP)) return Forbid();
            return View(await _context.Categories.Alive().Include(c => c.Products).ToListAsync());
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            if (!CheckRoles(ADMIN | SHOP)) return Forbid();
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            if (!CheckRoles(ADMIN | SHOP)) return Forbid();
            if (_context.Categories.Alive().Any(c => c.Name == category.Name))
                ModelState.AddModelError("Name", "Така категорія вже існує");
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.AliveFindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            if (id != category.Id || category.Deleted)
            {
                return NotFound();
            }

            if (_context.Categories.Alive().Any(c => c.Name == category.Name && c.Id != category.Id))
                ModelState.AddModelError("Name", "Така категорія вже існує");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckRoles(ADMIN | SHOP)) return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.Alive()
                .Include(t => t.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null || category.HasAlive)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckRoles(ADMIN | SHOP)) return Forbid();
            var category = await _context.Categories.FindAsync(id);
            category.Deleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
