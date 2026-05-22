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
    public class LoyaltyAccountsController : Controller
    {
        private readonly AppDbContext _context;

        public LoyaltyAccountsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LoyaltyAccounts
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.LoyaltyAccounts.Include(l => l.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: LoyaltyAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loyaltyAccount = await _context.LoyaltyAccounts
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loyaltyAccount == null)
            {
                return NotFound();
            }

            return View(loyaltyAccount);
        }

        // GET: LoyaltyAccounts/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: LoyaltyAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Points,Level,UserId")] LoyaltyAccount loyaltyAccount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loyaltyAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", loyaltyAccount.UserId);
            return View(loyaltyAccount);
        }

        // GET: LoyaltyAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loyaltyAccount = await _context.LoyaltyAccounts.FindAsync(id);
            if (loyaltyAccount == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", loyaltyAccount.UserId);
            return View(loyaltyAccount);
        }

        // POST: LoyaltyAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Points,Level,UserId")] LoyaltyAccount loyaltyAccount)
        {
            if (id != loyaltyAccount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loyaltyAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoyaltyAccountExists(loyaltyAccount.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", loyaltyAccount.UserId);
            return View(loyaltyAccount);
        }

        // GET: LoyaltyAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loyaltyAccount = await _context.LoyaltyAccounts
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loyaltyAccount == null)
            {
                return NotFound();
            }

            return View(loyaltyAccount);
        }

        // POST: LoyaltyAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loyaltyAccount = await _context.LoyaltyAccounts.FindAsync(id);
            if (loyaltyAccount != null)
            {
                _context.LoyaltyAccounts.Remove(loyaltyAccount);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoyaltyAccountExists(int id)
        {
            return _context.LoyaltyAccounts.Any(e => e.Id == id);
        }
    }
}
