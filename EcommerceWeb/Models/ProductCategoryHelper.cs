namespace EcommerceWeb.Models
{
    public class ProductCategoryHelper
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public Product Product { get; set; }
        public string CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
