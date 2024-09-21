using EcommerceWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace EcommerceWeb.ViewModels
{
    public class OrderViewModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public OrderStatus Status { get; set; }
        public string BillingAddressId { get; set; }
        public Address BillingAddress { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
