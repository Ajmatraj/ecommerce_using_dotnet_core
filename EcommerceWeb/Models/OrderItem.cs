using Microsoft.AspNetCore.Identity;

namespace EcommerceWeb.Models
{
    public class OrderItem
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public string OrderId { get; set; }
        private Order Order {  get; set; }
    }
}
