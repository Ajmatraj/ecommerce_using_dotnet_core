namespace EcommerceWeb.Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool HasDiscount { get; set; }
        public decimal Discount { get; set;}

    }
}
