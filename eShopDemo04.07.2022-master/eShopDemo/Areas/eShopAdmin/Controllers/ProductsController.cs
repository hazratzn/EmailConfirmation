using eShopDemo.Data;
using eShopDemo.Models;
using eShopDemo.Utilities.File;
using eShopDemo.Utilities.Helpers;
using eShopDemo.ViewModels.Admin;
using eShopDemo.ViewModels.ProductVMs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eShopDemo.Areas.eShopAdmin.Controllers
{
    [Area("eShopAdmin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHost;

        public ProductsController(AppDbContext context, IWebHostEnvironment webHost)
        {
            _context = context;
            _webHost = webHost;
        }

        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Products.Include(p => p.Category);
            return View(await appDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateVM productVM)
        {
            if (!ModelState.IsValid) return View();

            foreach (var photo in productVM.Photos)
            {
                if (!photo.CheckFileType("image/"))
                {
                    ModelState.AddModelError("Photo", "Image type is not correct.");
                    return View();
                }
                if (!photo.CheckFileSize(32))
                {
                    ModelState.AddModelError("Photo", "Image size should not over 32 megabyte.");
                    return View();
                }
            }

            List<ProductImage> productImages = new List<ProductImage>();

            foreach (var photo in productVM.Photos)
            {
                string filename = Guid.NewGuid().ToString() + "_" + photo.FileName;
                string path = Helper.GetFilePath(_webHost.WebRootPath, "images", filename);
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
                ProductImage image = new ProductImage();
                {
                    image.Image = filename;
                }
                productImages.Add(image);
            }

            productImages.FirstOrDefault().IsMain = true;

            Product product = new Product
            {
                Id = productVM.Id,
                Name = productVM.Name,
                ActualPrice = productVM.ActualPrice,
                DiscountPrice = productVM.DiscountPrice,
                Rating = productVM.Rating,
                UnitsQuantity = productVM.UnitsQuantity,
                UnitsSold = productVM.UnitsSold,
                CategoryId = productVM.CategoryId,
                ProductImages = productImages,
            };

            _context.Add(product);
            await _context.SaveChangesAsync();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .FirstOrDefaultAsync();
            if (product == null) return NotFound();

            ProductUpdateVM productVM = new ProductUpdateVM()
            {
                Id = product.Id,
                ProductImages = product.ProductImages,
                ActualPrice = product.ActualPrice,
                Name = product.Name,
                CategoryId = product.CategoryId,
                DiscountPrice = product.DiscountPrice,
                Rating = product.Rating,
                UnitsQuantity = product.UnitsQuantity,
                UnitsSold = product.UnitsSold
            };
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", productVM.CategoryId);

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductUpdateVM productVM)
        {
            if (id != productVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products
                        .Where(p => p.Id == id)
                        .Include(p => p.ProductImages)
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync();
                    if (product == null) return NotFound();

                    List<ProductImage> productImages = new List<ProductImage>();

                    if (productVM.Photos != null)
                    {
                        foreach (var photo in productVM.Photos)
                        {
                            if (!photo.CheckFileType("image/"))
                            {
                                ModelState.AddModelError("Photo", "Image type is not correct.");
                                return View();
                            }
                            if (!photo.CheckFileSize(32))
                            {
                                ModelState.AddModelError("Photo", "Image size should not over 32 megabyte.");
                                return View();
                            }
                        }
                        foreach (var photo in product.ProductImages)
                        {
                            string path = Helper.GetFilePath(_webHost.WebRootPath, "images", photo.Image);
                            Helper.DeleteFile(path);
                            photo.IsDeleted = true;
                        }

                        foreach (var photo in productVM.Photos)
                        {
                            string filename = Guid.NewGuid().ToString() + "_" + photo.FileName;
                            string newPath = Helper.GetFilePath(_webHost.WebRootPath, "images", filename);
                            using (FileStream stream = new FileStream(newPath, FileMode.Create))
                            {
                                await photo.CopyToAsync(stream);
                            }
                            ProductImage image = new ProductImage();
                            {
                                image.Image = filename;
                            }
                            productImages.Add(image);
                        }
                        productImages.FirstOrDefault().IsMain = true;
                        productVM.ProductImages = productImages;
                    }
                    
                    product.Name = productVM.Name;
                    product.ActualPrice = productVM.ActualPrice;
                    product.DiscountPrice = productVM.DiscountPrice;
                    product.Rating = productVM.Rating;
                    product.UnitsQuantity = productVM.UnitsQuantity;
                    product.UnitsSold = productVM.UnitsSold;
                    product.CategoryId = productVM.CategoryId;
                    product.ProductImages = productImages;
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(productVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productVM);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SetMainImage (DefaultImageVM defaultImage)
        {
            var productImages = await _context.ProductImages.Where(i => i.ProductId == defaultImage.ProductId).ToListAsync();
            foreach (var image in productImages)
            {
                if(image.Id == defaultImage.ImageId)
                {
                    image.IsMain = true;
                }
                else
                {
                    image.IsMain = false;
                }
                //defaultImage.IsMain = image.IsMain;
                //defaultImage.Image = image.Image;
            }
            
            await _context.SaveChangesAsync();
            return Ok(productImages);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
