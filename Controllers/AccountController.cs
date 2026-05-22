using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Data;
using Projet.Models;
using Projet.ViewModels;

namespace Projet.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ══════════════════════════════════════
        // INSCRIPTION
        // ══════════════════════════════════════
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool exists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);
            if (exists)
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Client",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.LoyaltyAccounts.Add(new LoyaltyAccount
            {
                UserId = user.Id,
                Points = 0,
                Level = "Bronze"
            });
            await _context.SaveChangesAsync();

            SetSession(user);

            TempData["Success"] = $"Bienvenue {user.FullName} ! Votre compte a été créé.";
            return RedirectToAction("Index", "Home");
        }

        // ══════════════════════════════════════
        // CONNEXION
        // ══════════════════════════════════════
        public IActionResult Login(string? returnUrl)
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Email ou mot de passe incorrect.");
                return View(model);
            }

            SetSession(user);

            TempData["Success"] = $"Bon retour {user.FullName} !";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }

        // ══════════════════════════════════════
        // DÉCONNEXION
        // ══════════════════════════════════════
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Vous avez été déconnecté(e).";
            return RedirectToAction("Index", "Home");
        }

        // ══════════════════════════════════════
        // PROFIL
        // ══════════════════════════════════════
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users
                .Include(u => u.LoyaltyAccount)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int Id, string FullName, string Email)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(Id);
            if (user == null || user.Id != userId) return NotFound();

            bool emailTaken = await _context.Users
                .AnyAsync(u => u.Email == Email && u.Id != Id);
            if (emailTaken)
            {
                TempData["Error"] = "Cet email est déjà utilisé.";
                return RedirectToAction("Profile");
            }

            user.FullName = FullName;
            user.Email = Email;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);

            TempData["Success"] = "Profil mis à jour avec succès !";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string CurrentPassword,
            string NewPassword,
            string ConfirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login");

            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.Password))
            {
                TempData["Error"] = "Mot de passe actuel incorrect.";
                return RedirectToAction("Profile");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "Les mots de passe ne correspondent pas.";
                return RedirectToAction("Profile");
            }

            if (NewPassword.Length < 6)
            {
                TempData["Error"] = "Le mot de passe doit contenir au moins 6 caractères.";
                return RedirectToAction("Profile");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Mot de passe modifié avec succès !";
            return RedirectToAction("Profile");
        }

        // ══════════════════════════════════════
        // MES COMMANDES
        // ══════════════════════════════════════
        public async Task<IActionResult> Orders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var orders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // ── Détail commande ────────────────────
        public async Task<IActionResult> OrderDetail(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id
                                       && o.UserId == userId);

            if (order == null) return NotFound();
            return View(order);
        }

        // ── Annuler commande ───────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            if (order.Status != "En attente")
            {
                TempData["Error"] = "Cette commande ne peut plus être annulée " +
                                    "car elle est déjà en cours de traitement.";
                return RedirectToAction("OrderDetail", new { id });
            }

            var items = await _context.OrderItems
                .Include(i => i.Product)
                .Where(i => i.OrderId == id)
                .ToListAsync();

            foreach (var item in items)
            {
                if (item.Product != null)
                    item.Product.Stock += item.Quantity;
            }

            var loyalty = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (loyalty != null)
            {
                int pointsToRemove = (int)(order.Total * 10);
                loyalty.Points = Math.Max(0, loyalty.Points - pointsToRemove);
                if (loyalty.Points >= 10000) loyalty.Level = "Platinum";
                else if (loyalty.Points >= 5000) loyalty.Level = "Gold";
                else if (loyalty.Points >= 1000) loyalty.Level = "Silver";
                else loyalty.Level = "Bronze";

                _context.LoyaltyTransactions.Add(new LoyaltyTransaction
                {
                    LoyaltyAccountId = loyalty.Id,
                    Points = pointsToRemove,
                    Type = "Débit",
                    Reason = $"Annulation commande #{order.Id}",
                    CreatedAt = DateTime.Now
                });
            }

            order.Status = "Annulée";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Commande #{order.Id} annulée avec succès.";
            return RedirectToAction("Orders");
        }

        // ══════════════════════════════════════
        // POINTS FIDÉLITÉ
        // ══════════════════════════════════════
        public async Task<IActionResult> Loyalty()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var loyalty = await _context.LoyaltyAccounts
                .Include(l => l.Transactions)
                .FirstOrDefaultAsync(l => l.UserId == userId);

            return View(loyalty);
        }

        // ══════════════════════════════════════
        // MES AVIS
        // ══════════════════════════════════════
        public async Task<IActionResult> Reviews()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            // Mes avis existants
            var reviews = await _context.Reviews
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // IDs des produits déjà évalués
            var reviewedProductIds = reviews
                .Select(r => r.ProductId)
                .ToList();

            // Commandes livrées du client
            var deliveredOrders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId && o.Status == "Livrée")
                .ToListAsync();

            // Produits achetés et livrés sans avis
            var purchasedProducts = deliveredOrders
                .SelectMany(o => o.Items)
                .Where(i => i.Product != null
                         && !reviewedProductIds.Contains(i.ProductId))
                .Select(i => i.Product!)
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();

            ViewBag.PurchasedProducts = purchasedProducts;

            return View(reviews);
        }

        // ── Laisser un avis ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(
            int productId, int rating, string comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            bool exists = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);

            if (exists)
            {
                TempData["Error"] = "Vous avez déjà laissé un avis pour ce produit.";
                return RedirectToAction("Reviews");
            }

            bool purchased = await _context.Orders
                .Include(o => o.Items)
                .AnyAsync(o => o.UserId == userId
                            && o.Status == "Livrée"
                            && o.Items.Any(i => i.ProductId == productId));

            if (!purchased)
            {
                TempData["Error"] = "Vous pouvez uniquement laisser un avis " +
                                    "sur un produit acheté et livré.";
                return RedirectToAction("Reviews");
            }

            _context.Reviews.Add(new Review
            {
                UserId = userId.Value,
                ProductId = productId,
                Rating = Math.Clamp(rating, 1, 5),
                Comment = comment,
                IsApproved = false,
                CreatedAt = DateTime.Now
            });

            var loyalty = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (loyalty != null)
            {
                loyalty.Points += 50;
                if (loyalty.Points >= 10000) loyalty.Level = "Platinum";
                else if (loyalty.Points >= 5000) loyalty.Level = "Gold";
                else if (loyalty.Points >= 1000) loyalty.Level = "Silver";
                else loyalty.Level = "Bronze";

                _context.LoyaltyTransactions.Add(new LoyaltyTransaction
                {
                    LoyaltyAccountId = loyalty.Id,
                    Points = 50,
                    Type = "Crédit",
                    Reason = "Avis produit laissé",
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Avis envoyé ! Il sera publié après modération. +50 pts 🌸";
            return RedirectToAction("Reviews");
        }

        // ── Supprimer un avis ──────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Avis supprimé.";
            }

            return RedirectToAction("Reviews");
        }

        // ══════════════════════════════════════
        // HELPER SESSION
        // ══════════════════════════════════════
        private void SetSession(User user)
        {
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);
        }
    }
}