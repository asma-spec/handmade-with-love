using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Data;
using Projet.Models;

namespace Projet.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        // ── Historique chat ────────────────────
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var messages = await _context.ChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        // ── Envoyer message ────────────────────
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Non connecté" });

            // Sauvegarder message client
            var clientMsg = new ChatMessage
            {
                Message = dto.Message,
                SenderType = "Client",
                UserId = userId.Value,
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.ChatMessages.Add(clientMsg);
            await _context.SaveChangesAsync();

            // Réponse automatique du bot
            string botReply = GetBotReply(dto.Message);
            var botMsg = new ChatMessage
            {
                Message = botReply,
                SenderType = "Bot",
                UserId = userId.Value,
                SentAt = DateTime.Now.AddSeconds(1),
                IsRead = true
            };
            _context.ChatMessages.Add(botMsg);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                reply = botReply,
                sentAt = botMsg.SentAt.ToString("HH:mm")
            });
        }

        // ── Envoyer message anonyme (visiteur) ─
        [HttpPost]
        public IActionResult SendAnonymous([FromBody] SendMessageDto dto)
        {
            string botReply = GetBotReply(dto.Message);
            return Json(new
            {
                success = true,
                reply = botReply,
                sentAt = DateTime.Now.ToString("HH:mm")
            });
        }

        // ── Marquer comme lu ───────────────────
        [HttpPost]
        public async Task<IActionResult> MarkRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false });

            var unread = await _context.ChatMessages
                .Where(m => m.UserId == userId && !m.IsRead)
                .ToListAsync();

            unread.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ── Chatbot IA ─────────────────────────
        private string GetBotReply(string message)
        {
            string msg = message.ToLower().Trim();

            if (msg.Contains("livraison") || msg.Contains("délai") || msg.Contains("expédition"))
                return "🚚 Nos délais de livraison sont de 24 à 48h partout en Tunisie. " +
                       "Vous recevrez un email de confirmation avec votre numéro de suivi.";

            if (msg.Contains("retour") || msg.Contains("remboursement") || msg.Contains("échange"))
                return "🔄 Vous avez 30 jours pour retourner un article. " +
                       "Contactez-nous par email à contact@accessoryhub.com avec votre numéro de commande.";

            if (msg.Contains("paiement") || msg.Contains("carte") || msg.Contains("virement"))
                return "💳 Nous acceptons les paiements par carte bancaire, virement et paiement à la livraison. " +
                       "Toutes les transactions sont 100% sécurisées.";

            if (msg.Contains("taille") || msg.Contains("guide") || msg.Contains("mesure"))
                return "📏 Vous trouverez notre guide des tailles sur chaque fiche produit. " +
                       "En cas de doute, n'hésitez pas à commander une taille au-dessus.";

            if (msg.Contains("promo") || msg.Contains("réduction") || msg.Contains("code"))
                return "🎁 Inscrivez-vous à notre newsletter pour recevoir des codes promo exclusifs ! " +
                       "Nouveau client ? Utilisez le code BIENVENUE10 pour -10% sur votre première commande.";

            if (msg.Contains("commande") || msg.Contains("suivi") || msg.Contains("statut"))
                return "📦 Pour suivre votre commande, rendez-vous dans votre espace client > " +
                       "Mes commandes. Vous y trouverez le statut en temps réel.";

            if (msg.Contains("compte") || msg.Contains("inscription") || msg.Contains("connexion"))
                return "👤 Pour créer un compte ou vous connecter, cliquez sur l'icône utilisateur " +
                       "en haut à droite de la page.";

            if (msg.Contains("fidélité") || msg.Contains("points") || msg.Contains("récompense"))
                return "⭐ Notre programme fidélité vous offre 10 points pour chaque 1 DT dépensé. " +
                       "4 niveaux : Bronze, Silver, Gold et Platinum avec des avantages exclusifs !";

            if (msg.Contains("bonjour") || msg.Contains("salut") || msg.Contains("bonsoir") || msg.Contains("hello"))
                return "👋 Bonjour et bienvenue chez AccessoryHub ! " +
                       "Comment puis-je vous aider aujourd'hui ?";

            if (msg.Contains("merci") || msg.Contains("super") || msg.Contains("parfait"))
                return "😊 Avec plaisir ! N'hésitez pas si vous avez d'autres questions. " +
                       "Bonne visite sur AccessoryHub !";

            if (msg.Contains("horaire") || msg.Contains("ouverture") || msg.Contains("disponible"))
                return "🕐 Notre support est disponible du lundi au samedi de 9h à 18h. " +
                       "En dehors de ces horaires, laissez-nous un message et nous vous répondrons dès que possible.";

            if (msg.Contains("contact") || msg.Contains("email") || msg.Contains("téléphone"))
                return "📞 Vous pouvez nous contacter par email : contact@accessoryhub.com " +
                       "ou par téléphone : +216 XX XXX XXX (Lun-Sam, 9h-18h).";

            return "🤔 Je ne suis pas sûr de comprendre votre question. " +
                   "Voici ce que je peux vous aider avec : livraison, retours, paiement, " +
                   "commandes, fidélité, tailles. Ou tapez 'contact' pour parler à un agent.";
        }
    }

    public class SendMessageDto
    {
        public string Message { get; set; } = string.Empty;
    }
}