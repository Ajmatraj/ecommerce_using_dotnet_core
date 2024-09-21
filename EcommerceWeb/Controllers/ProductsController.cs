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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceWeb.Controllers
{
    //for every controller
    [Authorize(Roles = "admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.products.ToListAsync());
        }

        // GET: Products/Details/5
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


        // GET: Products/Create
        public IActionResult Create()
        {
            //getting category throuh list.
            var category = _context.categories;
            ViewBag.Categories = new SelectList(category, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,HasDiscount,Discount,Images,Categories")] ProductViewModel product)
        {
            if (string.IsNullOrEmpty(product.Name) || string.IsNullOrEmpty(product.Description) || product.Price == 0)
            {
                return View(product);
            }

            // Create new Product with a new Product ID
            var productModel = new Product()
            {
                Id = Guid.NewGuid().ToString(),  // Generating the new Product ID here
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                HasDiscount = product.HasDiscount,
                Discount = product.Discount,
            };

            // Add the Product to the context
            _context.Add(productModel);

            // Saving Images
            foreach (var image in product.Images)
            {
                var imgPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                var uniqueName = $"{Guid.NewGuid()}_{image.FileName}";
                var filePath = Path.Combine(imgPath, uniqueName);

                using (var file = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(file);
                }

                var img = new Image()
                {
                    Id = Guid.NewGuid().ToString(),
                    imagePath = $"images/{uniqueName}"
                };

                // Save image entity
                _context.Add(img);

                // Save the relationship between Product and Image
                var imgProdHelper = new ImageProductHelper()
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = productModel.Id,  // Use productModel.Id (not product.Id)
                    ImageId = img.Id
                };

                _context.Add(imgProdHelper);
            }

            // Saving Categories
            foreach (var cat in product.Categories)
            {
                var proCatHelper = new ProductCategoryHelper()
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = productModel.Id,  // Use productModel.Id (not product.Id)
                    CategoryId = cat
                };

                _context.Add(proCatHelper);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.products == null)
            {
                return NotFound();
            }

            var product = await _context.products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            //getting only id form productCategoryHepler of prodct and category.
            var selectedCategory = _context.productCategoryHelpers.Where(x=>x.ProductId == product.Id).Select(x=>x.CategoryId).ToList();
            var category = _context.categories;
            ViewBag.Categories = new SelectList(category,"Id","Name");
          
            return View(new ProductViewModel()
            {
                Id= Guid.NewGuid().ToString(),
                Categories = selectedCategory,
                Description = product.Description,
                HasDiscount = product.HasDiscount,
                Discount = product.Discount,
                Name = product.Name,
                Price = product.Price,
            });

        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Description,Price,HasDiscount,Discount,Images,Categories")] ProductViewModel product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(product.Name) || string.IsNullOrEmpty(product.Description) || product.Price == 0)
            {
                return View(product);
            }

            // Create new Product with a new Product ID
            var productModel = _context.products.FirstOrDefault(x => x.Id == product.Id);
            if (productModel == null)
            {
                return NotFound();
            }


            productModel.Name = product.Name;
            productModel.Description = product.Description;
            productModel.Price = product.Price;
            productModel.HasDiscount = product.HasDiscount;
            productModel.Discount = product.Discount;

            // update the Product to the context
            _context.Update(productModel);

            if(product.Images != null)
            {
                //removing existing relation.
                var imgs = _context.imageProductHelpers.Where(x=>x.ProductId == product.Id);
                _context.imageProductHelpers.RemoveRange(imgs);

                // Saving Images and new relations adding.
                foreach (var image in product.Images)
                {
                    var imgPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    var uniqueName = $"{Guid.NewGuid()}_{image.FileName}";
                    var filePath = Path.Combine(imgPath, uniqueName);

                    var file = new FileStream(filePath, FileMode.Create);
                    
                    await image.CopyToAsync(file);
                    
                    file.Close();
                    var img = new Image()
                    {
                        Id = Guid.NewGuid().ToString(),
                        imagePath = $"images/{uniqueName}"
                    };

                    // Save image entity
                    _context.Add(img);

                    // Save the relationship between Product and Image
                    var imgProdHelper = new ImageProductHelper()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = productModel.Id,  // Use productModel.Id (not product.Id)
                        ImageId = img.Id
                    };

                    _context.Add(imgProdHelper);
                }
            }
            
            var cats = _context.productCategoryHelpers.Where(x=>x.ProductId != product.Id);
            _context.productCategoryHelpers.RemoveRange(cats);
            // Saving Categories
            foreach (var cat in product.Categories)
            {
                var proCatHelper = new ProductCategoryHelper()
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = productModel.Id,  // Use productModel.Id (not product.Id)
                    CategoryId = cat
                };

                _context.Add(proCatHelper);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.products == null)
            {
                return NotFound();
            }

            var product = await _context.products
                .FirstOrDefaultAsync(m => m.Id == id);
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

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var product = await _context.products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            //deleting the categories form the helper table.(if any)
            var productCategories = _context.productCategoryHelpers.Where(x => x.ProductId == product.Id).ToList();
            if (productCategories.Any())
            {
                _context.productCategoryHelpers.RemoveRange(productCategories);
            }

            //deleting the Images form the helper table.(if any)
            var productImgs = _context.imageProductHelpers.Where(x => x.ProductId == product.Id).ToList();
            if (productImgs.Any())
            {
                _context.imageProductHelpers.RemoveRange(productImgs);
            }

            //finaaly removing the product.
            _context.products.Remove(product);

            //save changes to database.
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(string id)
        {
            return _context.products.Any(e => e.Id == id);
        }
    }
}
