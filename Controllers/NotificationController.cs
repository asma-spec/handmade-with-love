using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Data;

namespace Projet.Controllers
{
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        // ── Liste des notifications ───────────
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Marquer toutes comme lues
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _context.SaveChangesAsync();

            // Récupérer toutes les notifs
            var notifs = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifs);
        }

        // ── Nombre non lues (AJAX) ────────────
        public async Task<IActionResult> UnreadCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { count = 0 });

            int count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Json(new { count });
        }

        // ── Marquer une notif comme lue ───────
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id
                                       && n.UserId == userId);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(notif.Link))
                    return Redirect(notif.Link);
            }

            return RedirectToAction("Index");
        }
    }
}