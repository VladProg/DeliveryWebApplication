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
    public class TrademarksController : MyController
    {
        private readonly DeliveryContext _context;

        public TrademarksController(DeliveryContext context, UserManager<User> userManager)
            : base(userManager)
            => _context = context;

        // GET: Trademarks
        public async Task<IActionResult> Index()
        {
            if (!CheckRoles(ADMIN|SHOP)) return Forbid();
            return View(await _context.Trademarks.Alive().Include(t => t.Products).ToListAsync());
        }

        // GET: Trademarks/Create
        public IActionResult Create()
        {
            if (!CheckRoles(ADMIN | SHOP)) return Forbid();
            return View();
        }

        // POST: Trademarks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Trademark trademark)
        {
            if (!CheckRoles(ADMIN | SHOP)) return Forbid();
            if (_context.Trademarks.Alive().Any(t => t.Name == trademark.Name))
                ModelState.AddModelError("Name", "Така торгова марка вже існує");
            if (ModelState.IsValid)
            {
                _context.Add(trademark);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trademark);
        }

        // GET: Trademarks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var trademark = await _context.Trademarks.AliveFindAsync(id);
            if (trademark == null)
            {
                return NotFound();
            }
            return View(trademark);
        }

        // POST: Trademarks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Trademark trademark)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            if (id != trademark.Id || trademark.Deleted)
            {
                return NotFound();
            }

            if (_context.Trademarks.Alive().Any(t => t.Name == trademark.Name && t.Id != trademark.Id))
                ModelState.AddModelError("Name", "Така торгова марка вже існує");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trademark);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrademarkExists(trademark.Id))
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
            return View(trademark);
        }

        // GET: Trademarks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckRoles(ADMIN | SHOP)) return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var trademark = await _context.Trademarks.Alive()
                .Include(t => t.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trademark == null || trademark.HasAlive)
            {
                return NotFound();
            }

            return View(trademark);
        }

        // POST: Trademarks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckRoles(ADMIN | SHOP)) return Forbid();
            var trademark = await _context.Trademarks.FindAsync(id);
            trademark.Deleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrademarkExists(int id)
        {
            return _context.Trademarks.Any(e => e.Id == id);
        }
    }
}
