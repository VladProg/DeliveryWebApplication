using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DeliveryWebApplication.ViewModels
{
    public class ChangeRoleViewModel
    {
        public User User { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsCourier { get; set; }
        [Display(Name = "Представник магазину")]
        public int? ShopId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
