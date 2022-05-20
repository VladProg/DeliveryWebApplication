using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryWebApplication
{
    public class MyController : Controller
    {
        public UserManager<User> UserManager { get; set; }

        public MyController(UserManager<User> userManager)
        {
            Utils.SetCulture();
            UserManager = userManager;
        }

        public const int CUSTOMER = 1 << 0;
        public const int COURIER = 1 << 1;
        public const int SHOP = 1 << 2;
        public const int ADMIN = 1 << 3;
        public const int UNAUTHORIZED = 1 << 4;
        public const int AUTHORIZED = CUSTOMER | COURIER | SHOP | ADMIN;
        public const int ANY = AUTHORIZED | UNAUTHORIZED;

        public bool CheckRoles(int mask = 0, int? CUSTOMER = null, int? COURIER = null, int? SHOP = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.IsAuthenticated = false;
                return (mask & MyController.UNAUTHORIZED) != 0;
            }

            ViewBag.IsAuthenticated = true;
            ViewBag.Name = User.Identity.Name;
            User user = UserManager.FindByNameAsync(ViewBag.Name).Result;

            ViewBag.Roles = UserManager.GetRolesAsync(user).Result;
            ViewBag.UserCustomerId = user.CustomerId;
            ViewBag.UserCourierId = user.CourierId;
            ViewBag.UserShopId = user.ShopId;

            if (TempData.Peek("Role") is null || !ViewBag.Roles.Contains((string)TempData.Peek("Role")))
            {
                TempData["Role"] = "none";
                if (ViewBag.Roles.Contains("customer")) TempData["Role"] = "customer";
                if (ViewBag.Roles.Contains("courier")) TempData["Role"] = "courier";
                if (ViewBag.Roles.Contains("shop")) TempData["Role"] = "shop";
                if (ViewBag.Roles.Contains("admin")) TempData["Role"] = "admin";
            }

            if ((string)TempData.Peek("Role") == "customer")
            {
                TempData["CustomerId"] = ViewBag.UserCustomerId;
                TempData["CourierId"] = 0;
            }
            if ((string)TempData.Peek("Role") == "courier")
            {
                TempData["CustomerId"] = 0;
                TempData["CourierId"] = ViewBag.UserCourierId;
            }

            if ((string)TempData.Peek("Role") == "none") return (mask & MyController.UNAUTHORIZED) != 0;
            if ((string)TempData.Peek("Role") == "customer")
                return CUSTOMER is null ? (mask & MyController.CUSTOMER) != 0 : CUSTOMER == ViewBag.UserCustomerId;
            if ((string)TempData.Peek("Role") == "courier")
                return COURIER is null ? (mask & MyController.COURIER) != 0 : COURIER == ViewBag.UserCourierId;
            if ((string)TempData.Peek("Role") == "shop")
                return SHOP is null ? (mask & MyController.SHOP) != 0 : SHOP == ViewBag.UserShopId;
            if ((string)TempData.Peek("Role") == "admin")
                return (mask & MyController.ADMIN) != 0;
            throw new NotImplementedException("TempData[\"Role\"] == \"" + (string)TempData.Peek("Role") + "\"");
        }
    }
}
