using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceWeb.Data;
using EcommerceWeb.Models;
using EcommerceWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EcommerceWeb.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart
       
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Fetch cart items for the logged-in user
            var cartItems = await _context.carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            var response = new List<CartViewModel>();

            foreach (var item in cartItems)
            {
                response.Add(new CartViewModel
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Product = new ProductViewModel
                    {
                        Name = item.Product.Name,
                        Price = item.Product.HasDiscount
                            ? item.Product.Price - (item.Product.Price * item.Product.Discount / 100)
                            : item.Product.Price,
                        Quantity = item.Quantity,
                    }
                });
            }

            return View(response);
        }

        // POST: AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart([Bind("Id", "Quantity")] ProductViewModel product)
        {
            if (product == null)
            {
                return BadRequest("Invalid product data.");
            }

            var userId = _userManager.GetUserId(User);

            // Fetch the product in the user's cart if it exists
            var cartProd = await _context.carts
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == product.Id);

            if (cartProd != null)
            {
                // Update quantity if the product is already in the cart
                cartProd.Quantity += product.Quantity ?? 1;
                _context.carts.Update(cartProd);
            }
            else
            {
                // Add new product to the cart if it doesn't exist
                var newCartItem = new Cart
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.Id,
                    UserId = userId,
                    Quantity = product.Quantity ?? 1,
                };
                await _context.carts.AddAsync(newCartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Shop", new { id = product.Id });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartQuantity([Bind("Id", "Quantity")] ProductViewModel product)
        {

            if (product == null)
            {
                return NotFound();
            }

            product.Quantity ??= 0;
            var userId = _userManager?.GetUserId(User);
            var cartProd = _context.carts.Where(x=>x.UserId == userId && x.ProductId == product.Id).FirstOrDefault();

            if (cartProd != null)
            {
                cartProd.Quantity = (int)product.Quantity;
                _context.Update(cartProd);
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Cart", new { id = product.Id });
        }

        // POST: RemoveFromCart
        
        public async Task<IActionResult> RemoveFromCart(String id)
        {
          
            var userId = _userManager.GetUserId(User);

            // Find the product in the cart to remove it
            var cartProd = _context.carts.Where(x => x.UserId == userId && x.ProductId == id).FirstOrDefault();

            if (cartProd != null)
            {
                _context.carts.Remove(cartProd);
                await _context.SaveChangesAsync();
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
