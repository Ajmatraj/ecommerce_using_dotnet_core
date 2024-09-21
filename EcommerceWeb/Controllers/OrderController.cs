using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcommerceWeb.Data;
using EcommerceWeb.Models;
using Microsoft.AspNetCore.Identity;
using EcommerceWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public OrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Order
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Orders.Include(o => o.BillingAddress).Include(o => o.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.BillingAddress)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            var items = _context.OrderItems.Include(x=>x.Product).Where(x=>x.OrderId == id).ToList();

            return View(new OrderViewModel { Id = order.Id, User = order.User, BillingAddress= order.BillingAddress, Items = items });
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            var cartItem = _context.carts.Include(x=>x.Product).Where(x=>x.UserId == userId).ToList();
            var items = new List<OrderItem>();

            cartItem.ForEach(item =>
            {
                items.Add(new OrderItem
                {
                    Quantity = item.Quantity,
                    Product = new Product
                    {
                        Name = item.Product.Name,
                        Price = item.Product.HasDiscount
                            ? item.Product.Price - (item.Product.Price * item.Product.Discount / 100)
                            : item.Product.Price,
                      
                    }
                });
            });

           return View(new OrderViewModel { BillingAddress = new Address(),Items = items});
        }

        // POST: Order/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BillingAddress")] OrderViewModel order)
        {
            if(string.IsNullOrEmpty(order.BillingAddress.FirstName) || String.IsNullOrEmpty(order.BillingAddress.LastName) || 
                String.IsNullOrEmpty(order.BillingAddress.Email) || string.IsNullOrEmpty(order.BillingAddress.Country) || string.IsNullOrEmpty(order.BillingAddress.Address1) || String.IsNullOrEmpty(order.BillingAddress.Address2) || String.IsNullOrEmpty(order.BillingAddress.City) || String.IsNullOrEmpty(order.BillingAddress.State) || String.IsNullOrEmpty(order.BillingAddress.Phone))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            order.BillingAddress.Id = Guid.NewGuid().ToString();
            _context.Add(order.BillingAddress);

            var orderModel = new Order()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                BillingAddressId = order.BillingAddress.Id,
                Status = OrderStatus.Pending,
            };
            _context.Add(orderModel);

            var cartItem = _context.carts.Include(x => x.Product).Where(x => x.UserId == userId).ToList();

            cartItem.ForEach(item =>
            {
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = item.Quantity,
                    OrderId = orderModel.Id,
                    ProductId = item.ProductId,
                };
                _context.Add(orderItem);
                _context.Remove(item);
            });

           _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        // GET: Order/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["BillingAddressId"] = new SelectList(_context.Addresses, "Id", "Id", order.BillingAddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Order/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserId,Status,BillingAddressId")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }
            var orderModel = _context.Orders.Find(id);
            if (orderModel == null)
            {
                return NotFound();
            }

            orderModel.Status = order.Status;
            _context.Update(orderModel); 
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Order/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.BillingAddress)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            var items = _context.OrderItems.Where(x => x.OrderId == id).ToList();

            return View(new OrderViewModel { Id = order.Id, User = order.User, BillingAddress = order.BillingAddress, Items = items });
        }
    

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
