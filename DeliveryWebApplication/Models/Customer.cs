using System;
using System.Collections.Generic;

namespace DeliveryWebApplication
{
    public partial class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; }

        public string NameWithPhone => Name + ", " + Phone;
    }
}
