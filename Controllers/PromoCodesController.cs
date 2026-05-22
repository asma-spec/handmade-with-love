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
    public class PromoCodesController : Controller
    {
        private readonly AppDbContext _context;

        public PromoCodesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PromoCodes
        public async Task<IActionResult> Index()
        {
            return View(await _context.PromoCodes.ToListAsync());
        }

        // GET: PromoCodes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promoCode = await _context.PromoCodes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (promoCode == null)
            {
                return NotFound();
            }

            return View(promoCode);
        }

        // GET: PromoCodes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PromoCodes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Type,Value,MinOrderAmount,ExpiresAt,MaxUsage,UsageCount,IsActive")] PromoCode promoCode)
        {
            if (ModelState.IsValid)
            {
                _context.Add(promoCode);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(promoCode);
        }

        // GET: PromoCodes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promoCode = await _context.PromoCodes.FindAsync(id);
            if (promoCode == null)
            {
                return NotFound();
            }
            return View(promoCode);
        }

        // POST: PromoCodes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Type,Value,MinOrderAmount,ExpiresAt,MaxUsage,UsageCount,IsActive")] PromoCode promoCode)
        {
            if (id != promoCode.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promoCode);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromoCodeExists(promoCode.Id))
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
            return View(promoCode);
        }

        // GET: PromoCodes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promoCode = await _context.PromoCodes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (promoCode == null)
            {
                return NotFound();
            }

            return View(promoCode);
        }

        // POST: PromoCodes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promoCode = await _context.PromoCodes.FindAsync(id);
            if (promoCode != null)
            {
                _context.PromoCodes.Remove(promoCode);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PromoCodeExists(int id)
        {
            return _context.PromoCodes.Any(e => e.Id == id);
        }
    }
}
