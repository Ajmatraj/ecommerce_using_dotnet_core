using Microsoft.AspNetCore.Identity;

namespace EcommerceWeb.Models
{
    public class Order
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public OrderStatus Status { get; set; }
        public string BillingAddressId { get; set; }
        public Address BillingAddress { get; set; }
    }

    //enum
    public enum OrderStatus
    {
        Pending,
        Approved,
        Cancel,
        Complete
    }
}
