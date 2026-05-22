using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projet.Data;
using Projet.Models;

namespace projet.Controllers
{
    public class ChatMessagesController : Controller
    {
        private readonly AppDbContext _context;

        public ChatMessagesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ChatMessages
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ChatMessages.Include(c => c.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ChatMessages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatMessage = await _context.ChatMessages
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chatMessage == null)
            {
                return NotFound();
            }

            return View(chatMessage);
        }

        // GET: ChatMessages/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: ChatMessages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Message,SenderType,SentAt,IsRead,UserId")] ChatMessage chatMessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chatMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", chatMessage.UserId);
            return View(chatMessage);
        }

        // GET: ChatMessages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatMessage = await _context.ChatMessages.FindAsync(id);
            if (chatMessage == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", chatMessage.UserId);
            return View(chatMessage);
        }

        // POST: ChatMessages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Message,SenderType,SentAt,IsRead,UserId")] ChatMessage chatMessage)
        {
            if (id != chatMessage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chatMessage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChatMessageExists(chatMessage.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", chatMessage.UserId);
            return View(chatMessage);
        }

        // GET: ChatMessages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatMessage = await _context.ChatMessages
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chatMessage == null)
            {
                return NotFound();
            }

            return View(chatMessage);
        }

        // POST: ChatMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chatMessage = await _context.ChatMessages.FindAsync(id);
            if (chatMessage != null)
            {
                _context.ChatMessages.Remove(chatMessage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChatMessageExists(int id)
        {
            return _context.ChatMessages.Any(e => e.Id == id);
        }
    }
}
