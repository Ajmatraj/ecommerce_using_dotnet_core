using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcommerceWeb.Data;
using EcommerceWeb.Models;
using EcommerceWeb.ViewModels;

namespace EcommerceWeb.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Shop


        // GET: Shop/Index
        // GET: Shop

        public async Task<IActionResult> Index([FromQuery] string searchText, [FromQuery] string categoryId)
        {
            searchText ??= "";
            var productviewModel = new List<ProductViewModel>();
            if (string.IsNullOrEmpty(categoryId))
            {
                var products = _context.products.Where(x => x.Name.ToLower().StartsWith(searchText.ToLower())).ToList();

                products.ForEach(product =>
                {
                    var categories = _context.productCategoryHelpers.Include(x => x.Category).Where(x => x.ProductId == product.Id).Select(x => x.Category.Name).ToList();
                    var img = _context.imageProductHelpers.Include(x => x.Image).Where(x => x.ProductId == product.Id).Select(x => x.Image.imagePath).ToList();
                    productviewModel.Add(new ProductViewModel
                    {
                        Name = product.Name,
                        Id = product.Id,
                        Description = product.Description,
                        Discount = product.Discount,
                        HasDiscount = product.HasDiscount,
                        Price = product.Price,
                        Categories = categories,
                        ImagePath = img
                    });
                });
            }
            else
            {
                var catProduct = _context.productCategoryHelpers.Include(x => x.Product).Where(x => x.CategoryId == categoryId && x.Product.Name.ToLower().StartsWith(searchText.ToLower())).ToList();
                catProduct.ForEach(cat =>
                {
                    var categories = _context.productCategoryHelpers.Include(x => x.Category).Where(x => x.ProductId == cat.ProductId).Select(x => x.Category.Name).ToList();
                    var img = _context.imageProductHelpers.Include(x => x.Image).Where(x => x.ProductId == cat.ProductId).Select(x => x.Image.imagePath).ToList();
                    productviewModel.Add(new ProductViewModel
                    {
                        Name = cat.Product.Name,
                        Id = cat.Product.Id,
                        Description = cat.Product.Description,
                        Discount = cat.Product.Discount,
                        HasDiscount = cat.Product.HasDiscount,
                        Price = cat.Product.Price,
                        Categories = categories,
                        ImagePath = img
                    });
                });
            }
            return View(productviewModel);
        }


        // GET: Shop/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.products == null)
            {
                return NotFound();
            }

            var product = await _context.products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Fetch category names instead of just CategoryId
            var selectedCategory = _context.productCategoryHelpers
                .Where(x => x.ProductId == product.Id)
                .Select(x => _context.categories.FirstOrDefault(c => c.Id == x.CategoryId).Name).ToList();

            var selectedImgs = _context.imageProductHelpers
                .Include(x => x.Image)
                .Where(x => x.ProductId == product.Id)
                .Select(x => x.Image.imagePath)
                .ToList();

            return View(new ProductViewModel()
            {
                Id = id,
                Categories = selectedCategory,
                ImagePath = selectedImgs,
                Description = product.Description,
                HasDiscount = product.HasDiscount,
                Discount = product.Discount,
                Name = product.Name,
                Price = product.Price,
               
            });
        }




    }
}
