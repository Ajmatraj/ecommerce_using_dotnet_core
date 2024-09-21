using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceWeb.ViewModels
{
    public class CarouselViewModel
    {
        public string Id { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string ButtonUrl { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }
    }
}

