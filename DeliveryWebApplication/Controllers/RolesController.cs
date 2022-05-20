using DeliveryWebApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DeliveryWebApplication.Controllers
{
    public class RolesController : MyController
    {
        public readonly UserManager<User> _userManager;
        private readonly DeliveryContext _context;

        public RolesController(UserManager<User> userManager, DeliveryContext context)
            : base(userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            return View(_userManager.Users.ToList());
        }

        public async Task<IActionResult> Edit(string userId)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                ChangeRoleViewModel model = new ChangeRoleViewModel
                {
                    User=user,
                    IsCustomer = userRoles.Contains("customer"),
                    IsCourier = userRoles.Contains("courier"),
                    ShopId = user.ShopId,
                    IsAdmin = userRoles.Contains("admin")
                };
                ViewData["ShopId"] = new SelectList(_context.Shops.OrderBy(x => x.Deleted).ThenBy(x => x.Name + " " + x.Address), "Id", "NameWithAddress", user.ShopId);
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string userId, ChangeRoleViewModel model)
        {
            if (!CheckRoles(ADMIN)) return Forbid();
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (model.IsCustomer)
                {
                    await _userManager.AddToRoleAsync(user, "customer");
                    if (user.CustomerId is null)
                    {
                        Customer customer = new() { Name = user.Name, Phone = user.PhoneNumber };
                        _context.Customers.Add(customer);
                        _context.SaveChanges();
                        user.CustomerId = customer.Id;
                        await _userManager.UpdateAsync(user);
                    }
                }
                else
                    await _userManager.RemoveFromRoleAsync(user, "customer");
                if (model.IsCourier)
                {
                    await _userManager.AddToRoleAsync(user, "courier");
                    if (user.CourierId is null)
                    {
                        Courier courier = new() { Name = user.Name, Phone = user.PhoneNumber };
                        _context.Couriers.Add(courier);
                        _context.SaveChanges();
                        user.CourierId = courier.Id;
                        await _userManager.UpdateAsync(user);
                    }
                }
                else
                    await _userManager.RemoveFromRoleAsync(user, "courier");
                if (model.ShopId is not null and not 0)
                {
                    await _userManager.AddToRoleAsync(user, "shop");
                    user.ShopId = model.ShopId;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, "shop");
                    user.ShopId = null;
                    await _userManager.UpdateAsync(user);
                }
                if (model.IsAdmin)
                    await _userManager.AddToRoleAsync(user, "admin");
                else
                    await _userManager.RemoveFromRoleAsync(user, "admin");
                return RedirectToAction("Index");
            }

            return NotFound();
        }

    }
}
