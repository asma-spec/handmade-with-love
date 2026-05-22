using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Data;
using Projet.Models;

namespace Projet.Controllers
{
    public class WishlistController : Controller
    {
        private readonly AppDbContext _context;

        public WishlistController(AppDbContext context)
        {
            _context = context;
        }

        // ── Liste Wishlist ─────────────────────
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account",
                    new { returnUrl = "/Wishlist" });

            var items = await _context.WishlistItems
                .Include(w => w.Product)
                    .ThenInclude(p => p.Category)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            return View(items);
        }

        // ── Ajouter à la Wishlist ──────────────
        public async Task<IActionResult> Add(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account",
                    new { returnUrl = $"/Wishlist/Add/{id}" });

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Vérifier si déjà dans la wishlist
            bool exists = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == id);

            if (!exists)
            {
                _context.WishlistItems.Add(new WishlistItem
                {
                    UserId = userId.Value,
                    ProductId = id,
                    AddedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = $"« {product.Name} » ajouté à votre wishlist !";
            }
            else
            {
                TempData["Error"] = "Ce produit est déjà dans votre wishlist.";
            }

            // Retourner à la page précédente
            string? referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index");
        }

        // ── Retirer de la Wishlist ─────────────
        public async Task<IActionResult> Remove(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == id);

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Produit retiré de la wishlist.";
            }

            return RedirectToAction("Index");
        }

        // ── Vider la Wishlist ──────────────────
        public async Task<IActionResult> Clear()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var items = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .ToListAsync();

            _context.WishlistItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Wishlist vidée.";
            return RedirectToAction("Index");
        }

        // ── Déplacer vers le panier ────────────
        public async Task<IActionResult> MoveToCart(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Retirer de la wishlist
            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == id);

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            // Ajouter au panier
            return RedirectToAction("Add", "Cart", new { id });
        }

        // ── Vérifier si produit dans wishlist ──
        public async Task<IActionResult> Check(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { inWishlist = false });

            bool exists = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

            return Json(new { inWishlist = exists });
        }
    }
}