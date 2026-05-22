using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Data;
using Projet.Models;
using System.Text.Json;

namespace Projet.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private const string CartKey = "Cart";

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // ══════════════════════════════════════
        // HELPERS PANIER
        // ══════════════════════════════════════

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json)
                  ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartKey,
                JsonSerializer.Serialize(cart));
        }

        private void AddItem(CartItem item)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(
                c => c.ProductId == item.ProductId);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                cart.Add(item);
            SaveCart(cart);
        }

        private void UpdateQuantityInCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(
                c => c.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            SaveCart(cart);
        }

        private void RemoveItem(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCart(cart);
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(CartKey);
        }

        private int CountItems() =>
            GetCart().Sum(c => c.Quantity);

        private decimal GetTotal() =>
            GetCart().Sum(c => c.Total);

        // ══════════════════════════════════════
        // ACTIONS
        // ══════════════════════════════════════

        // ── Afficher panier ────────────────────
        public IActionResult Index()
        {
            var items = GetCart();
            ViewBag.Total = GetTotal();
            ViewBag.Count = CountItems();
            return View(items);
        }

        // ── Ajouter au panier ──────────────────
        public async Task<IActionResult> Add(int id, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive)
            {
                TempData["Error"] = "Produit introuvable.";
                return RedirectToAction("Index", "Shop");
            }

            if (product.Stock < quantity)
            {
                TempData["Error"] = "Stock insuffisant.";
                return RedirectToAction("Details", "Shop", new { id });
            }

            // ── Prix promo ─────────────────────
            decimal unitPrice = product.Price;
            if (product.IsOnSale && product.DiscountPercent > 0)
            {
                unitPrice = product.Price *
                    (1 - (product.DiscountPercent ?? 0) / 100);
            }

            AddItem(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = unitPrice,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            });

            TempData["Success"] =
                $"« {product.Name} » ajouté au panier !";
            return RedirectToAction("Index");
        }

        // ── Modifier quantité ──────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(
            int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Produit introuvable.";
                return RedirectToAction("Index");
            }

            if (quantity > product.Stock)
            {
                TempData["Error"] =
                    $"Stock insuffisant pour « {product.Name} ». " +
                    $"Maximum disponible : {product.Stock}.";
                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                RemoveItem(productId);
                TempData["Success"] = "Article retiré du panier.";
                return RedirectToAction("Index");
            }

            UpdateQuantityInCart(productId, quantity);
            return RedirectToAction("Index");
        }

        // ── Supprimer article ──────────────────
        public IActionResult Remove(int productId)
        {
            RemoveItem(productId);
            TempData["Success"] = "Article retiré du panier.";
            return RedirectToAction("Index");
        }

        // ── Vider panier ───────────────────────
        public IActionResult Clear()
        {
            ClearCart();
            TempData["Success"] = "Panier vidé.";
            return RedirectToAction("Index");
        }

        // ── Appliquer code promo ───────────────
        [HttpPost]
        public async Task<IActionResult> ApplyPromo(string code)
        {
            var promo = await _context.PromoCodes
                .FirstOrDefaultAsync(p =>
                    p.Code == code.ToUpper() &&
                    p.IsActive &&
                    p.UsageCount < p.MaxUsage &&
                    (p.ExpiresAt == null ||
                     p.ExpiresAt > DateTime.Now));

            if (promo == null)
            {
                TempData["Error"] = "Code promo invalide ou expiré.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetString("PromoCode", promo.Code);
            HttpContext.Session.SetString("PromoType", promo.Type);
            HttpContext.Session.SetInt32("PromoValue", (int)promo.Value);
            TempData["Success"] = $"Code « {promo.Code} » appliqué !";
            return RedirectToAction("Index");
        }

        // ── Retirer code promo ─────────────────
        public IActionResult RemovePromo()
        {
            HttpContext.Session.Remove("PromoCode");
            HttpContext.Session.Remove("PromoType");
            HttpContext.Session.Remove("PromoValue");
            return RedirectToAction("Index");
        }

        // ── Checkout ───────────────────────────
        public IActionResult Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account",
                    new { returnUrl = "/Cart/Checkout" });

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Votre panier est vide.";
                return RedirectToAction("Index");
            }

            decimal total = GetTotal();
            decimal discount = 0;
            string? promoType = HttpContext.Session.GetString("PromoType");
            int promoValue = HttpContext.Session.GetInt32("PromoValue") ?? 0;

            if (promoType == "Percentage")
                discount = total * promoValue / 100;
            else if (promoType == "FixedAmount")
                discount = promoValue;

            // ── Réduction fidélité ─────────────────
            decimal loyaltyDiscount = 0;
            var loyalty = _context.LoyaltyAccounts
                .FirstOrDefault(l => l.UserId == userId);

            if (loyalty != null)
            {
                if (loyalty.Level == "Silver")
                    loyaltyDiscount = total * 5 / 100;
                else if (loyalty.Level == "Gold")
                    loyaltyDiscount = total * 10 / 100;
                else if (loyalty.Level == "Platinum")
                    loyaltyDiscount = total * 15 / 100;
            }

            ViewBag.Cart = cart;
            ViewBag.Total = total;
            ViewBag.PromoCode = HttpContext.Session.GetString("PromoCode");
            ViewBag.Discount = discount;
            ViewBag.LoyaltyDiscount = loyaltyDiscount;
            ViewBag.LoyaltyLevel = loyalty?.Level ?? "Bronze";
            ViewBag.FinalTotal = total - discount - loyaltyDiscount;

            return View();
        }
        // ── Confirmer commande ─────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(
    string shippingAddress, string paymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var cartItems = GetCart();
            if (!cartItems.Any())
            {
                TempData["Error"] = "Votre panier est vide.";
                return RedirectToAction("Index");
            }

            // ── Vérification stock ─────────────────
            foreach (var item in cartItems)
            {
                var product = await _context.Products
                    .FindAsync(item.ProductId);

                if (product == null || !product.IsActive)
                {
                    TempData["Error"] =
                        $"Le produit « {item.ProductName} » " +
                        $"n'est plus disponible.";
                    return RedirectToAction("Index");
                }

                if (item.Quantity > product.Stock)
                {
                    TempData["Error"] =
                        $"Stock insuffisant pour « {product.Name} ». " +
                        $"Disponible : {product.Stock}, " +
                        $"demandé : {item.Quantity}.";
                    return RedirectToAction("Index");
                }
            }

            // ── Calcul remise code promo ───────────
            decimal discount = 0;
            string? promoCode = HttpContext.Session.GetString("PromoCode");
            string? promoType = HttpContext.Session.GetString("PromoType");
            int promoValue = HttpContext.Session
                .GetInt32("PromoValue") ?? 0;
            decimal total = GetTotal();

            if (promoType == "Percentage")
                discount = total * promoValue / 100;
            else if (promoType == "FixedAmount")
                discount = promoValue;

            // ── Réduction fidélité ─────────────────
            decimal loyaltyDiscount = 0;
            var loyalty = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (loyalty != null)
            {
                if (loyalty.Level == "Silver")
                    loyaltyDiscount = total * 5 / 100;
                else if (loyalty.Level == "Gold")
                    loyaltyDiscount = total * 10 / 100;
                else if (loyalty.Level == "Platinum")
                    loyaltyDiscount = total * 15 / 100;
            }

            decimal totalDiscount = discount + loyaltyDiscount;
            decimal finalTotal = total - totalDiscount;
            if (finalTotal < 0) finalTotal = 0;

            // ── Récupérer user ─────────────────────
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // ── Créer commande ─────────────────────
            var order = new Order
            {
                UserId = userId.Value,
                CustomerName = user.FullName,
                CustomerEmail = user.Email,
                ShippingAddress = shippingAddress,
                Total = finalTotal,
                Discount = totalDiscount,
                PromoCodeUsed = promoCode,
                Status = "En attente",
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // ── OrderItems + décrémenter stock ─────
            foreach (var item in cartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });

                var product = await _context.Products
                    .FindAsync(item.ProductId);
                if (product != null)
                    product.Stock -= item.Quantity;
            }

            // ── Code promo usage ───────────────────
            if (!string.IsNullOrEmpty(promoCode))
            {
                var promo = await _context.PromoCodes
                    .FirstOrDefaultAsync(p => p.Code == promoCode);
                if (promo != null) promo.UsageCount++;
            }

            // ── Points fidélité ────────────────────
            if (loyalty != null)
            {
                int points = (int)(finalTotal * 10);
                loyalty.Points += points;

                if (loyalty.Points >= 10000)
                    loyalty.Level = "Platinum";
                else if (loyalty.Points >= 5000)
                    loyalty.Level = "Gold";
                else if (loyalty.Points >= 1000)
                    loyalty.Level = "Silver";
                else
                    loyalty.Level = "Bronze";

                _context.LoyaltyTransactions.Add(
                    new LoyaltyTransaction
                    {
                        LoyaltyAccountId = loyalty.Id,
                        Points = points,
                        Type = "Crédit",
                        Reason = $"Commande #{order.Id}",
                        CreatedAt = DateTime.Now
                    });

                // ── Transaction réduction fidélité ─
                if (loyaltyDiscount > 0)
                {
                    _context.LoyaltyTransactions.Add(
                        new LoyaltyTransaction
                        {
                            LoyaltyAccountId = loyalty.Id,
                            Points = 0,
                            Type = "Réduction",
                            Reason = $"Réduction {loyalty.Level} " +
                                               $"appliquée (-{loyaltyDiscount:0.00} DT)" +
                                               $" sur commande #{order.Id}",
                            CreatedAt = DateTime.Now
                        });
                }
            }

            await _context.SaveChangesAsync();

            // ── Vider panier et promo ──────────────
            ClearCart();
            HttpContext.Session.Remove("PromoCode");
            HttpContext.Session.Remove("PromoType");
            HttpContext.Session.Remove("PromoValue");

            TempData["Success"] =
                $"Commande #{order.Id} passée avec succès !";
            return RedirectToAction("Confirmation",
                new { id = order.Id });
        }

        // ── Confirmation ───────────────────────
        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // ── API : Nombre d'articles ────────────
        public IActionResult GetCount()
        {
            return Json(new { count = CountItems() });
        }
    }
}