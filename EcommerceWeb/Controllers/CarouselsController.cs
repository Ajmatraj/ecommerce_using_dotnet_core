using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcommerceWeb.Data;
using EcommerceWeb.Models;
using EcommerceWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceWeb.Controllers
{
    //for every controller
    [Authorize(Roles = "admin")]
    public class CarouselsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CarouselsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Carousels
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Carousels.Include(c => c.Image);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Carousels/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Carousels == null)
            {
                return NotFound();
            }

            var carousel = await _context.Carousels
                .Include(c => c.Image)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carousel == null)
            {
                return NotFound();
            }

            return View(carousel);
        }

        // GET: Carousels/Create
        public IActionResult Create()
        {
            ViewData["ImageId"] = new SelectList(_context.images, "Id", "Id");
            return View();
        }

        // POST: Carousels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Heading,SubHeading,ButtonUrl,Image")] CarouselViewModel carousel)
        {
            if (string.IsNullOrEmpty(carousel.Heading) || string.IsNullOrEmpty(carousel.SubHeading) || string.IsNullOrEmpty(carousel.ButtonUrl) || carousel.Image == null)
            {
                return View(carousel);
            }

            var carouselModel = new Carousel()
            {
                Id = Guid.NewGuid().ToString(),
                Heading = carousel.Heading,
                SubHeading = carousel.SubHeading,
                ButtonUrl = carousel.ButtonUrl,
            };

            // File path management for image upload and save in the "images" folder.
            var imgPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");

            var uniqueName = $"{Guid.NewGuid()}_{carousel.Image.FileName}";
            var filePath = Path.Combine(imgPath, uniqueName);

            // Use a "using" block to properly dispose of the FileStream
            using (var file = new FileStream(filePath, FileMode.Create))
            {
                await carousel.Image.CopyToAsync(file);
            }

            var img = new Image()
            {
                Id = Guid.NewGuid().ToString(),
                imagePath = $"images/{uniqueName}" // Store only the relative path
            };

            _context.Add(img);
            carouselModel.ImageId = img.Id;
            _context.Add(carouselModel);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Carousels/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Carousels == null)
            {
                return NotFound();
            }

            var carousel = await _context.Carousels.FindAsync(id);
            if (carousel == null)
            {
                return NotFound();
            }
            return View(new CarouselViewModel { ButtonUrl = carousel.ButtonUrl, Heading = carousel.Heading, SubHeading = carousel.SubHeading, Id = carousel.Id });
        }

        // POST: Carousels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Heading,SubHeading,ButtonUrl,Image")] CarouselViewModel carousel)
        {
            if (id != carousel.Id)
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(carousel.Heading) || string.IsNullOrEmpty(carousel.SubHeading) || string.IsNullOrEmpty(carousel.ButtonUrl))
            {
                return View(carousel);
            }

            var carouselModel = await _context.Carousels.FindAsync(id);
            carouselModel.SubHeading = carousel.SubHeading;
            carouselModel.Heading = carousel.Heading;
            carouselModel.ButtonUrl = carousel.ButtonUrl;

            if (carousel.Image != null)
            {
                // Manage file path for image upload and save in root "images" folder.
                var imgPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                var uniqueName = $"{Guid.NewGuid()}_{carousel.Image.FileName}";
                var filePath = Path.Combine(imgPath, uniqueName);

                // Use a "using" block to properly dispose of the FileStream
                using (var file = new FileStream(filePath, FileMode.Create))
                {
                    await carousel.Image.CopyToAsync(file);
                }

                var img = new Image()
                {
                    Id = Guid.NewGuid().ToString(),
                    imagePath = $"images/{uniqueName}" // Store only the relative path
                };

                _context.Add(img);
                carouselModel.ImageId = img.Id;
            }

            _context.Update(carouselModel);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Carousels/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carousel = await _context.Carousels
                .Include(c => c.Image)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carousel == null)
            {
                return NotFound();
            }

            return View(carousel);
        }

        // POST: Carousels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var carousel = await _context.Carousels.FindAsync(id);
            if (carousel != null)
            {
                _context.Carousels.Remove(carousel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarouselExists(string id)
        {
            return _context.Carousels.Any(e => e.Id == id);
        }
    }
}
