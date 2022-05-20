using DeliveryWebApplication;
using Microsoft.AspNetCore.Identity;

namespace DeliveryWebApplication
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            string adminEmail = "admin@everyday.com.ua";
            string password = "123456";
            if (await roleManager.FindByNameAsync("customer") == null)
                await roleManager.CreateAsync(new IdentityRole("customer"));
            if (await roleManager.FindByNameAsync("courier") == null)
                await roleManager.CreateAsync(new IdentityRole("courier"));
            if (await roleManager.FindByNameAsync("shop") == null)
                await roleManager.CreateAsync(new IdentityRole("shop"));
            if (await roleManager.FindByNameAsync("admin") == null)
                await roleManager.CreateAsync(new IdentityRole("admin"));
            if (!userManager.Users.Any())
            {
                User admin = new User { Email = adminEmail, UserName = adminEmail, PhoneNumber = "-", Name = "Адмін" };
                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "admin");
            }
        }

    }
}
