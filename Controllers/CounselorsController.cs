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
            if (id == null)
            {
                return NotFound();
            }

            var counselor = await _context.Counselors
                .FirstOrDefaultAsync(m => m.CounselorID == id);
            if (counselor == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Create([Bind("CounselorID,EmployeeNumber,FirstName,MiddleName,LastName,Gender,BirthDate,ContactNumber,EmailAddress,Address,EducationalAttainment,LicenseNumber,YearsOfExperience,Position,DateHired,EmploymentStatus")] Counselor counselor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(counselor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(counselor);
        }

        // GET: Counselors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var counselor = await _context.Counselors.FindAsync(id);
            if (counselor == null)
            {
                return NotFound();
            }
            return View(counselor);
        }

        // POST: Counselors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CounselorID,EmployeeNumber,FirstName,MiddleName,LastName,Gender,BirthDate,ContactNumber,EmailAddress,Address,EducationalAttainment,LicenseNumber,YearsOfExperience,Position,DateHired,EmploymentStatus")] Counselor counselor)
        {
            if (id != counselor.CounselorID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(counselor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CounselorExists(counselor.CounselorID))
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
            return View(counselor);
        }

        // GET: Counselors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var counselor = await _context.Counselors
                .FirstOrDefaultAsync(m => m.CounselorID == id);
            if (counselor == null)
            {
                return NotFound();
            }

            return View(counselor);
        }

        // POST: Counselors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var counselor = await _context.Counselors.FindAsync(id);
            if (counselor != null)
            {
                _context.Counselors.Remove(counselor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CounselorExists(int id)
        {
            return _context.Counselors.Any(e => e.CounselorID == id);
        }
    }
}
