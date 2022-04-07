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
    public class TrademarksController : Controller
    {
        private readonly DeliveryContext _context;

        public TrademarksController(DeliveryContext context)
        {
            _context = context;
        }

        // GET: Trademarks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Trademarks.Include(t => t.Products).ToListAsync());
        }

        // GET: Trademarks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trademark = await _context.Trademarks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trademark == null)
            {
                return NotFound();
            }

            return View(trademark);
        }

        // GET: Trademarks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Trademarks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Trademark trademark)
        {
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
            if (id == null)
            {
                return NotFound();
            }

            var trademark = await _context.Trademarks.FindAsync(id);
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
            if (id != trademark.Id)
            {
                return NotFound();
            }

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
            if (id == null)
            {
                return NotFound();
            }

            var trademark = await _context.Trademarks
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trademark == null)
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
            var trademark = await _context.Trademarks.FindAsync(id);
            _context.Trademarks.Remove(trademark);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrademarkExists(int id)
        {
            return _context.Trademarks.Any(e => e.Id == id);
        }
    }
}
