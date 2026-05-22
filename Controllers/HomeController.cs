using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Data;

namespace Projet.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _context.Categories.Take(6).ToListAsync();
            ViewBag.FeaturedProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8).ToListAsync();
            ViewBag.NewProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4).ToListAsync();
            ViewBag.PromoCode = await _context.PromoCodes
                .Where(p => p.IsActive)
                .FirstOrDefaultAsync();
            ViewBag.Reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Take(3).ToListAsync();
            ViewBag.PromoProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsOnSale && p.DiscountPercent > 0)
                .OrderByDescending(p => p.DiscountPercent)
                .Take(4)
                .ToListAsync();
            return View();

        }
    }
}