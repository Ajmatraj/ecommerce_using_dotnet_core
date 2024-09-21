
namespace EcommerceWeb.Models
{
    public class Carousel
    {
        public string Id { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string ButtonUrl { get; set; }
        public string ImageId { get; set; }
        public Image Image { get; set; }

       
    }
}
