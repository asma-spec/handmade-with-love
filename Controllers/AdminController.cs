using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Data;

namespace Projet.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ── Vérification Admin ─────────────────
        private bool IsAdmin() =>
            HttpContext.Session.GetString("UserRole") == "Admin";

        private IActionResult RedirectIfNotAdmin() =>
            RedirectToAction("Login", "Account");

        // ══════════════════════════════════════
        // TABLEAU DE BORD
        // ══════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            // ── KPIs de base ──────────────────
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalUsers = await _context.Users
                .CountAsync(u => u.Role == "Client");
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status != "Annulée")
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            // ── Commandes par statut ──────────
            ViewBag.PendingOrders = await _context.Orders
                .CountAsync(o => o.Status == "En attente");
            ViewBag.ShippedOrders = await _context.Orders
                .CountAsync(o => o.Status == "Expédiée");
            ViewBag.DeliveredOrders = await _context.Orders
                .CountAsync(o => o.Status == "Livrée");
            ViewBag.CancelledOrders = await _context.Orders
                .CountAsync(o => o.Status == "Annulée");

            // ── Nouveaux utilisateurs ce mois ─
            ViewBag.NewUsers = await _context.Users
                .CountAsync(u => u.CreatedAt.Month == DateTime.Now.Month
                              && u.CreatedAt.Year == DateTime.Now.Year);

            // ── CA par mois (12 derniers mois) ─
            var caParMois = await _context.Orders
                .Where(o => o.Status != "Annulée"
                         && o.CreatedAt >= DateTime.Now.AddMonths(-11))
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new {
                    Annee = g.Key.Year,
                    Mois = g.Key.Month,
                    Total = g.Sum(o => o.Total)
                })
                .OrderBy(g => g.Annee)
                .ThenBy(g => g.Mois)
                .ToListAsync();
            ViewBag.CaParMois = caParMois;

            // ── Commandes par mois ────────────
            var cmdParMois = await _context.Orders
                .Where(o => o.CreatedAt >= DateTime.Now.AddMonths(-11))
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new {
                    Annee = g.Key.Year,
                    Mois = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(g => g.Annee)
                .ThenBy(g => g.Mois)
                .ToListAsync();
            ViewBag.CmdParMois = cmdParMois;

            // ── Top 5 produits les plus vendus ─
            var topProduits = await _context.OrderItems
                .Include(i => i.Product)
                .GroupBy(i => new {
                    i.ProductId,
                    i.Product.Name,
                    i.Product.ImageUrl
                })
                .Select(g => new {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    ImageUrl = g.Key.ImageUrl,
                    TotalVendu = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(g => g.TotalVendu)
                .Take(5)
                .ToListAsync();
            ViewBag.TopProduits = topProduits;

            // ── Top 2 catégories ──────────────
            var topCategories = await _context.OrderItems
                .Include(i => i.Product)
                    .ThenInclude(p => p.Category)
                .GroupBy(i => new {
                    i.Product.CategoryId,
                    i.Product.Category.Name
                })
                .Select(g => new {
                    CategoryId = g.Key.CategoryId,
                    Name = g.Key.Name,
                    TotalVendu = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(g => g.TotalVendu)
                .Take(2)
                .ToListAsync();
            ViewBag.TopCategories = topCategories;

            // ── Top 3 clients (points) ─────────
            var topClients = await _context.LoyaltyAccounts
                .Include(l => l.User)
                .OrderByDescending(l => l.Points)
                .Take(3)
                .Select(l => new {
                    FullName = l.User.FullName,
                    Email = l.User.Email,
                    Points = l.Points,
                    Level = l.Level
                })
                .ToListAsync();
            ViewBag.TopClients = topClients;

            // ── Commandes récentes ─────────────
            ViewBag.RecentOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            // ── Stock critique ─────────────────
            ViewBag.LowStock = await _context.Products
                .Where(p => p.Stock <= 5 && p.IsActive)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // ══════════════════════════════════════
        // GESTION PRODUITS
        // ══════════════════════════════════════
        public async Task<IActionResult> Products(
            string? search, int? categoryId)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync());
        }

        public async Task<IActionResult> ToggleProduct(int id)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = product.IsActive
                    ? "Produit activé." : "Produit désactivé.";
            }
            return RedirectToAction("Products");
        }

        // ══════════════════════════════════════
        // GESTION COMMANDES
        // ══════════════════════════════════════
        public async Task<IActionResult> Orders(
            string? status, string? search)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(o =>
                    o.CustomerName.Contains(search) ||
                    o.CustomerEmail.Contains(search));

            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.Statuses = new[]
            {
                "En attente", "Confirmée", "En préparation",
                "Expédiée", "Livrée", "Annulée"
            };

            return View(await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync());
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(
            int id, string status)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Statut mis à jour : {status}";
            }
            return RedirectToAction("OrderDetails", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTracking(
            int id, string trackingNumber)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.TrackingNumber = trackingNumber;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Numéro de suivi mis à jour !";
            }
            return RedirectToAction("OrderDetails", new { id });
        }

        // ══════════════════════════════════════
        // GESTION UTILISATEURS
        // ══════════════════════════════════════
        public async Task<IActionResult> Users(
            string? search, string? role)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var query = _context.Users
                .Include(u => u.LoyaltyAccount)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search));

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role == role);

            ViewBag.Search = search;
            ViewBag.Role = role;

            return View(await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync());
        }

        public async Task<IActionResult> ToggleUser(int id)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = user.IsActive
                    ? "Utilisateur activé."
                    : "Utilisateur désactivé.";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> AddPoints(
            int userId, int points)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var loyalty = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (loyalty != null)
            {
                loyalty.Points += points;
                if (loyalty.Points >= 10000) loyalty.Level = "Platinum";
                else if (loyalty.Points >= 5000) loyalty.Level = "Gold";
                else if (loyalty.Points >= 1000) loyalty.Level = "Silver";
                else loyalty.Level = "Bronze";

                _context.LoyaltyTransactions.Add(
                    new Models.LoyaltyTransaction
                    {
                        LoyaltyAccountId = loyalty.Id,
                        Points = points,
                        Type = points > 0 ? "Crédit" : "Débit",
                        Reason = "Ajout manuel par l'administrateur",
                        CreatedAt = DateTime.Now
                    });

                await _context.SaveChangesAsync();
                TempData["Success"] = $"{points} points ajoutés avec succès.";
            }
            return RedirectToAction("Users");
        }

        // ══════════════════════════════════════
        // GESTION CATÉGORIES
        // ══════════════════════════════════════
        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var cats = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
            return View(cats);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(
            string name, string? description)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            _context.Categories.Add(new Models.Category
            {
                Name = name,
                Description = description
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Catégorie ajoutée !";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var cat = await _context.Categories.FindAsync(id);
            if (cat != null)
            {
                _context.Categories.Remove(cat);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Catégorie supprimée.";
            }
            return RedirectToAction("Categories");
        }

        // ══════════════════════════════════════
        // GESTION CODES PROMO
        // ══════════════════════════════════════
        public async Task<IActionResult> PromoCodes()
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var promos = await _context.PromoCodes
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return View(promos);
        }

        [HttpPost]
        public async Task<IActionResult> AddPromoCode(
            string code, string type,
            decimal value, int maxUsage,
            DateTime? expiresAt)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            bool exists = await _context.PromoCodes
                .AnyAsync(p => p.Code == code);
            if (exists)
            {
                TempData["Error"] = "Ce code existe déjà.";
                return RedirectToAction("PromoCodes");
            }

            _context.PromoCodes.Add(new Models.PromoCode
            {
                Code = code.ToUpper(),
                Type = type,
                Value = value,
                MaxUsage = maxUsage,
                ExpiresAt = expiresAt,
                IsActive = true
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Code promo créé !";
            return RedirectToAction("PromoCodes");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePromoCode(int id)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var promo = await _context.PromoCodes.FindAsync(id);
            if (promo != null)
            {
                promo.IsActive = !promo.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PromoCodes");
        }

        // ══════════════════════════════════════
        // MODÉRATION AVIS
        // ══════════════════════════════════════
        public async Task<IActionResult> Reviews(string? filter)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .AsQueryable();

            if (filter == "pending")
                query = query.Where(r => !r.IsApproved);
            else if (filter == "approved")
                query = query.Where(r => r.IsApproved);

            ViewBag.Filter = filter;

            return View(await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ApproveReview(int id)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = !review.IsApproved;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Reviews");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (!IsAdmin()) return RedirectIfNotAdmin();

            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Avis supprimé.";
            }
            return RedirectToAction("Reviews");
        }
    }
}