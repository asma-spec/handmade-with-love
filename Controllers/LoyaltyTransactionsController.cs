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
    public class LoyaltyTransactionsController : Controller
    {
        private readonly AppDbContext _context;

        public LoyaltyTransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LoyaltyTransactions
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.LoyaltyTransactions.Include(l => l.LoyaltyAccount);
            return View(await appDbContext.ToListAsync());
        }

        // GET: LoyaltyTransactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loyaltyTransaction = await _context.LoyaltyTransactions
                .Include(l => l.LoyaltyAccount)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loyaltyTransaction == null)
            {
                return NotFound();
            }

            return View(loyaltyTransaction);
        }

        // GET: LoyaltyTransactions/Create
        public IActionResult Create()
        {
            ViewData["LoyaltyAccountId"] = new SelectList(_context.LoyaltyAccounts, "Id", "Id");
            return View();
        }

        // POST: LoyaltyTransactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Points,Type,Reason,CreatedAt,LoyaltyAccountId")] LoyaltyTransaction loyaltyTransaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loyaltyTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LoyaltyAccountId"] = new SelectList(_context.LoyaltyAccounts, "Id", "Id", loyaltyTransaction.LoyaltyAccountId);
            return View(loyaltyTransaction);
        }

        // GET: LoyaltyTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loyaltyTransaction = await _context.LoyaltyTransactions.FindAsync(id);
            if (loyaltyTransaction == null)
            {
                return NotFound();
            }
            ViewData["LoyaltyAccountId"] = new SelectList(_context.LoyaltyAccounts, "Id", "Id", loyaltyTransaction.LoyaltyAccountId);
            return View(loyaltyTransaction);
        }

        // POST: LoyaltyTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Points,Type,Reason,CreatedAt,LoyaltyAccountId")] LoyaltyTransaction loyaltyTransaction)
        {
            if (id != loyaltyTransaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loyaltyTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoyaltyTransactionExists(loyaltyTransaction.Id))
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
            ViewData["LoyaltyAccountId"] = new SelectList(_context.LoyaltyAccounts, "Id", "Id", loyaltyTransaction.LoyaltyAccountId);
            return View(loyaltyTransaction);
        }

        // GET: LoyaltyTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loyaltyTransaction = await _context.LoyaltyTransactions
                .Include(l => l.LoyaltyAccount)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loyaltyTransaction == null)
            {
                return NotFound();
            }

            return View(loyaltyTransaction);
        }

        // POST: LoyaltyTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loyaltyTransaction = await _context.LoyaltyTransactions.FindAsync(id);
            if (loyaltyTransaction != null)
            {
                _context.LoyaltyTransactions.Remove(loyaltyTransaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoyaltyTransactionExists(int id)
        {
            return _context.LoyaltyTransactions.Any(e => e.Id == id);
        }
    }
}
