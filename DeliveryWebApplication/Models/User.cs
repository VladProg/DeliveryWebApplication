using Microsoft.AspNetCore.Identity;

namespace DeliveryWebApplication
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public int? CustomerId { get; set; }
        public int? CourierId { get; set; }
        public int? ShopId { get; set; }
    }
}