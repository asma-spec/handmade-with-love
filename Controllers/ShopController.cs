using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Data;

namespace Projet.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }

        // ── Catalogue principal ────────────────
        public async Task<IActionResult> Index(
            int? categoryId, string? search,
            string? sort, decimal? minPrice,
            decimal? maxPrice, int page = 1)
        {
            int pageSize = 12;

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Filtre catégorie
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            // Filtre recherche
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));

            // Filtre prix
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);

            // Tri
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            int total = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = categoryId;
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total = total;

            return View(products);
        }

        // ── Recherche rapide ───────────────────
        public async Task<IActionResult> Search(string q)
        {
            return RedirectToAction(nameof(Index), new { search = q });
        }

        // ── Fiche produit ──────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return NotFound();

            // ── Vous aimerez aussi ────────────────
            // 3 produits de la même catégorie
            // sauf le produit actuel
            var youMayAlsoLike = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == product.CategoryId
                         && p.Id != id
                         && p.IsActive
                         && p.Stock > 0)
                .OrderBy(p => Guid.NewGuid()) // aléatoire
                .Take(3)
                .ToListAsync();

            ViewBag.YouMayAlsoLike = youMayAlsoLike;

            return View(product);
        }
        // ── Page catégories publique ───────────────
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products.Where(p => p.IsActive))
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }
    }
}