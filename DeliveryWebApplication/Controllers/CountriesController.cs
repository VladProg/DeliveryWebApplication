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
    public class CountriesController : MyController
    {
        private readonly DeliveryContext _context;

        public CountriesController(DeliveryContext context, UserManager<User> userManager)
            : base(userManager)
            => _context = context;

        // GET: Countries
        public async Task<IActionResult> Index()
        {
            if (!CheckRoles(ADMIN|SHOP)) return Forbid();
            return View(await _context.Countries.Alive().Include(t => t.Products).ToListAsync());
        }

        // GET: Countries/Create
        public IActionResult Create()
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            return View();
        }

        // POST: Countries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Country country)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            if (_context.Countries.Alive().Any(c => c.Name == country.Name))
                ModelState.AddModelError("Name", "Така країна вже існує");
            if (ModelState.IsValid)
            {
                _context.Add(country);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        // GET: Countries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries.AliveFindAsync(id);
            if (country == null)
            {
                return NotFound();
            }
            return View(country);
        }

        // POST: Countries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Country country)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            if (id != country.Id || country.Deleted)
            {
                return NotFound();
            }

            if (_context.Countries.Alive().Any(c => c.Name == country.Name && c.Id != country.Id))
                ModelState.AddModelError("Name", "Така країна вже існує");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(country);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CountryExists(country.Id))
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
            return View(country);
        }

        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries.Alive()
                .Include(t => t.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (country == null || country.HasAlive)
            {
                return NotFound();
            }

            return View(country);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            var country = await _context.Countries.FindAsync(id);
            country.Deleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }
    }
}
