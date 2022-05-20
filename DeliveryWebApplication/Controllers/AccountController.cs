using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DeliveryWebApplication.ViewModels;
using DeliveryWebApplication.Models;

namespace DeliveryWebApplication.Controllers
{
    public class AccountController : MyController
    {

        public readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly DeliveryContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, DeliveryContext context)
            : base(userManager)
        {
            _signInManager = signInManager;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            CheckRoles();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            CheckRoles();
            if (ModelState.IsValid)
            {
                Customer customer = new() { Name = model.Name, Phone = model.Phone };
                _context.Customers.Add(customer);
                _context.SaveChanges();
                User user = new User
                {
                    Email = model.Email,
                    UserName = model.Email,
                    Name = model.Name,
                    PhoneNumber=model.Phone,
                    CustomerId=customer.Id
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                await _userManager.AddToRoleAsync(user, "customer");
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description
                            .Replace("Username", "E-mail")
                            .Replace("is already taken", "вже зареєстрований")
                            .Replace("Passwords must be at least", "Пароль має містити принаймні")
                            .Replace("characters", "символів"));
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null, string email = null)
        {
            CheckRoles();
            return View(new LoginViewModel { ReturnUrl = returnUrl, Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            CheckRoles();
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    TempData["CustomerId"] = TempData["CourierId"] = 0;
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильний логін / пароль");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            CheckRoles();
            await _signInManager.SignOutAsync();
            TempData["CustomerId"] = TempData["CourierId"] = 0;
            TempData.Remove("Role");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Customer()
        {
            TempData["Role"] = "customer";
            if (!CheckRoles(CUSTOMER)) return Forbid();
            TempData["CustomerId"] = TempData["CourierId"] = 0;
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Courier()
        {
            TempData["Role"] = "courier";
            if (!CheckRoles(COURIER)) return Forbid();
            TempData["CustomerId"] = TempData["CourierId"] = 0;
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Shop()
        {
            TempData["Role"] = "shop";
            if (!CheckRoles(SHOP)) return Forbid();
            TempData["CustomerId"] = TempData["CourierId"] = 0;
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Admin()
        {
            TempData["Role"] = "admin";
            if (!CheckRoles(ADMIN)) return Forbid();
            TempData["CustomerId"] = TempData["CourierId"] = 0;
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            CheckRoles();
            return View();
        }

        public async Task<IActionResult> Edit()
        {
            if (!CheckRoles(AUTHORIZED)) return Forbid();
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            return View(new EditAccountViewModel { Name = user.Name, Phone = user.PhoneNumber, Email = user.Email });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            if (!CheckRoles(AUTHORIZED)) return Forbid();
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                user.Name = model.Name;
                user.PhoneNumber = model.Phone;
                user.Email = user.UserName = model.Email;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    if (user.CustomerId is not null)
                    {
                        var customer = await _context.Customers.FindAsync(user.CustomerId);
                        customer.Name = model.Name;
                        customer.Phone = model.Phone;
                        _context.Customers.Update(customer);
                        await _context.SaveChangesAsync();
                    }
                    if (user.CourierId is not null)
                    {
                        var courier = await _context.Couriers.FindAsync(user.CourierId);
                        courier.Name = model.Name;
                        courier.Phone = model.Phone;
                        _context.Couriers.Update(courier);
                        await _context.SaveChangesAsync();
                    }
                    if(model.Email!=User.Identity.Name)
                    {
                        await _signInManager.SignOutAsync();
                        TempData["CustomerId"] = TempData["CourierId"] = 0;
                        return RedirectToAction(nameof(Login), new { email = model.Email });
                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
                else
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description
                            .Replace("Username", "E-mail")
                            .Replace("is already taken", "вже зареєстрований"));
            }
            return View(model);
        }

        public IActionResult ChangePassword()
        {
            if (!CheckRoles(AUTHORIZED)) return Forbid();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!CheckRoles(AUTHORIZED)) return Forbid();
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByNameAsync(User.Identity.Name);
                IdentityResult result =
                    await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description
                            .Replace("Incorrect password", "Старий пароль неправильний")
                            .Replace("Passwords must be at least", "Пароль має містити принаймні")
                            .Replace("characters", "символів"));
                    }
                }
            }
            return View(model);
        }
    }
}
