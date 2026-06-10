using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GCAMS.Data;
using GCAMS.Models.Counselor;

namespace GCAMS.Controllers
{
    public class CounselorsController : Controller
    {
        private readonly AppDbContext _context;

        public CounselorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Counselors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Counselors.ToListAsync());
        }

        // GET: Counselors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var counselor = await _context.Counselors
                .Include(c => c.ContactNumbers)
                .FirstOrDefaultAsync(m => m.CounselorID == id);

            if (counselor == null) return NotFound();
            return View(counselor);
        }

        // GET: Counselors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Counselors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Counselor counselor)
        {
            if (ModelState.IsValid)
            {
                // Grab numbers before Add() touches them
                var incoming = counselor.ContactNumbers
                    .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                    .ToList();

                // Clear so Add() doesn't insert them
                counselor.ContactNumbers.Clear();

                _context.Counselors.Add(counselor);
                await _context.SaveChangesAsync();

                foreach (var c in incoming)
                {
                    _context.CounselorContactNumbers.Add(new CounselorContactNumber
                    {
                        CounselorID = counselor.CounselorID,
                        Number = c.Number,
                        Label = c.Label
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(counselor);
        }

        // GET: Counselors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var counselor = await _context.Counselors
                .Include(c => c.ContactNumbers)
                .FirstOrDefaultAsync(c => c.CounselorID == id);

            if (counselor == null) return NotFound();
            return View(counselor);
        }

        // POST: Counselors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Counselor counselor)
        {
            if (id != counselor.CounselorID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var incoming = counselor.ContactNumbers
                        .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                        .ToList();

                    counselor.ContactNumbers.Clear();

                    // Preserve the existing EmploymentStatus
                    var existing = await _context.Counselors
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.CounselorID == id);

                    if (existing == null) return NotFound();
                    counselor.EmploymentStatus = existing.EmploymentStatus;

                    _context.Update(counselor);

                    var old = _context.CounselorContactNumbers.Where(x => x.CounselorID == id);
                    _context.CounselorContactNumbers.RemoveRange(old);
                    await _context.SaveChangesAsync();

                    foreach (var c in incoming)
                    {
                        _context.CounselorContactNumbers.Add(new CounselorContactNumber
                        {
                            CounselorID = id,
                            Number = c.Number,
                            Label = c.Label
                        });
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Counselors.Any(e => e.CounselorID == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(counselor);
        }

        // GET: Counselors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var counselor = await _context.Counselors
                .FirstOrDefaultAsync(m => m.CounselorID == id);

            if (counselor == null) return NotFound();
            return View(counselor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var counselor = await _context.Counselors.FindAsync(id);
            if (counselor == null) return NotFound();

            counselor.EmploymentStatus = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Counselor has been set to inactive.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Counselors/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var counselor = await _context.Counselors.FindAsync(id);
            if (counselor == null) return NotFound();

            counselor.EmploymentStatus = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Counselor has been set to active.";
            return RedirectToAction(nameof(Index));
        }


        private bool CounselorExists(int id)
        {
            return _context.Counselors.Any(e => e.CounselorID == id);
        }
    }
}
