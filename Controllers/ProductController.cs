using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projet.Data;
using Projet.Models;

namespace Projet.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ── Liste ──────────────────────────────
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(products);
        }

        // ── Détails ────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // ── Créer ──────────────────────────────
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(
                _context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                    product.ImageUrl = await SaveImage(imageFile);

                product.CreatedAt = DateTime.Now;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // ── Notifier les clients intéressés ──
                await NotifyNewProductAsync(product);

                TempData["Success"] =
                    $"Produit « {product.Name} » ajouté !";
                return RedirectToAction("Products", "Admin");
            }

            ViewBag.CategoryId = new SelectList(
                _context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ── Modifier ───────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(
                _context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                    product.ImageUrl = await SaveImage(imageFile);
                else if (string.IsNullOrEmpty(product.ImageUrl))
                {
                    var existing = await _context.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);
                    product.ImageUrl = existing?.ImageUrl;
                }

                _context.Update(product);
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Produit « {product.Name} » modifié !";
                return RedirectToAction("Products", "Admin");
            }

            ViewBag.CategoryId = new SelectList(
                _context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ── Supprimer ──────────────────────────
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Produit supprimé.";
            }
            return RedirectToAction("Products", "Admin");
        }

        // ── Upload image ───────────────────────
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var folder = Path.Combine(
                _env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);
            var fileName = Guid.NewGuid().ToString()
                         + Path.GetExtension(imageFile.FileName);
            var path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await imageFile.CopyToAsync(stream);
            return "/images/products/" + fileName;
        }

        // ── Notifier clients intéressés ────────
        private async Task NotifyNewProductAsync(Product product)
        {
            var fullProduct = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (fullProduct == null) return;

            var wishlistItems = await _context.WishlistItems
                .Include(w => w.Product)
                .ToListAsync();

            var interestedUserIds = wishlistItems
                .Where(w => w.Product != null
                         && w.Product.CategoryId
                            == fullProduct.CategoryId)
                .Select(w => w.UserId)
                .Distinct()
                .ToList();

            if (!interestedUserIds.Any()) return;

            foreach (var userId in interestedUserIds)
            {
                bool alreadyNotified = await _context.Notifications
                    .AnyAsync(n => n.UserId == userId
                                && n.Link ==
                                   $"/Shop/Details/{fullProduct.Id}");

                if (alreadyNotified) continue;

                _context.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Title = $"Nouveau produit : {fullProduct.Name} !",
                    Message = $"Un nouveau produit dans la catégorie " +
                                $"« {fullProduct.Category?.Name} » " +
                                $"vient d'être ajouté.",
                    Icon = "fa-star",
                    Link = $"/Shop/Details/{fullProduct.Id}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}