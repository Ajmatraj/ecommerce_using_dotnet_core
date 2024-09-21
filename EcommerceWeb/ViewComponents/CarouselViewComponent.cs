using EcommerceWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWeb.ViewComponents
{
    public class CarouselViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public CarouselViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var carousels = await _context.Carousels.Include(x => x.Image).ToListAsync();
            return View(carousels);
        }
    }
}
