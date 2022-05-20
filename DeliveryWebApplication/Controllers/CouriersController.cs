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
    public class CouriersController : MyController
    {
        private readonly DeliveryContext _context;

        public CouriersController(DeliveryContext context, UserManager<User> userManager)
            : base(userManager)
            => _context = context;

        // GET: Couriers
        public async Task<IActionResult> Index()
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            return View(await _context.Couriers.ToListAsync());
        }

        // GET: Couriers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var courier = await _context.Couriers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courier == null)
            {
                return NotFound();
            }

            return View(courier);
        }

        // POST: Couriers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            var courier = await _context.Couriers.FindAsync(id);
            _context.Couriers.Remove(courier);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourierExists(int id)
        {
            return _context.Couriers.Any(e => e.Id == id);
        }
    }
}
