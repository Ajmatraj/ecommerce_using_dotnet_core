using EcommerceWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWeb.ViewComponents
{
    public class NavCategoryViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public NavCategoryViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var carousels = _context.categories.Include(x => x.Image);
            return View(carousels);
        }
    }
}
