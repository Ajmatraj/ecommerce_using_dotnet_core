using Microsoft.AspNetCore.Identity;

namespace EcommerceWeb.Models
{
    public class Cart
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public string ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }

    }
}
