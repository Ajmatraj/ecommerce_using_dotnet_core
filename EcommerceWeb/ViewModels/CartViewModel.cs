using EcommerceWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace EcommerceWeb.ViewModels
{
    public class CartViewModel
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public ProductViewModel Product { get; set; }
        public int Quantity { get; set; }
    }
}
