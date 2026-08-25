using GCAMS.Data;
using GCAMS.Models.ActivityLogs;
using GCAMS.Models.AnecRecs;
using GCAMS.Models.CaseNotes;
using GCAMS.Models.Students;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCAMS.Models.ActivityLogs;

namespace GCAMS.Controllers
{
    public class AnecRecsController : Controller
    {
        private readonly AppDbContext _context;

        public AnecRecsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AnecRecs
        public async Task<IActionResult> Index()
        {
            return View(await _context.AnecRecs.ToListAsync());
        }

        // GET: AnecRecs/Details/5
        public async Task<IActionResult> Details(int? id, int? studentId)
        {
            if (id == null) return NotFound();

            var anecrecs = await _context.AnecRecs.FirstOrDefaultAsync(m => m.AnecRecsId == id);
            if (anecrecs == null) return NotFound();

            ViewBag.StudentsID = studentId ?? anecrecs.StudentsID;
            return View(anecrecs);
        }

        // GET: AnecRecs/Create
        public async Task<IActionResult> Create(int? studentId)
        {
            var anecrecs = new AnecRecs();

            if (studentId.HasValue)
            {
                anecrecs.StudentsID = studentId.Value;

                var student = await _context.Students.FindAsync(studentId.Value);
                if (student != null) anecrecs.StuName = student.StuName;

                anecrecs.AnecRecNo = await _context.CaseNotes
                    .CountAsync(n => n.StudentsID == studentId.Value) + 1;
            }

            ModelState.Remove("AnecRecNo");
            ViewBag.StudentsID = studentId;
            return View(anecrecs);
        }

        // POST: AnecRecs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
         [Bind("AnecRecsId,StudentsID,StuName,DateOfObserv,ObservedBy,Place,PeopleInvolved,SceneMood,StudentBehavior,ObserverRecs")] AnecRecs anecRecs, int? studentId)
        {
            // fallback: only needed if the hidden field somehow didn't post
            if (anecRecs.StudentsID == 0 && studentId.HasValue)
                anecRecs.StudentsID = studentId.Value;

            if (anecRecs.StudentsID == 0)
            {
                ModelState.AddModelError("", "No student was specified for this record.");
                ViewBag.StudentsID = studentId;
                return View(anecRecs);
            }

            if (ModelState.IsValid)
            {
                _context.Add(anecRecs);
                await _context.SaveChangesAsync();
                // Activity Log
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Who = User.Identity?.Name ?? "Unknown",
                    Date = DateTime.Now,
                    ActivityAction = ActivityAction.AnnecRecCreated.ToString(),
                    Details = $"Anecdotal record #{anecRecs.AnecRecsId} was created for student {anecRecs.StuName}."
                });

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Students", new { id = anecRecs.StudentsID });
            }

            ViewBag.StudentsID = studentId;
            return View(anecRecs);
        }

        // GET: AnecRecs/Edit/5
        public async Task<IActionResult> Edit(int? id, int? studentId)
        {
            if (id == null) return NotFound();

            var anecRecs = await _context.AnecRecs.FindAsync(id);
            if (anecRecs == null) return NotFound();

            ViewBag.StudentsID = studentId ?? anecRecs.StudentsID;
            return View(anecRecs);
        }

        // POST: AnecRecs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AnecRecsId,StudentsID,StuName,DateOfObserv,ObservedBy,Place,PeopleInvolved,SceneMood,StudentBehavior,ObserverRecs")] AnecRecs anecRecs, int? studentId)
        {
            if (id != anecRecs.AnecRecsId) return NotFound();

            if (anecRecs.StudentsID == 0 && studentId.HasValue)
                anecRecs.StudentsID = studentId.Value;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(anecRecs);
                    await _context.SaveChangesAsync();

                    // Activity Log
                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Who = User.Identity?.Name ?? "Unknown",
                        Date = DateTime.Now,
                        ActivityAction = ActivityAction.AnnecRecUpdated.ToString(),
                        Details = $"Anecdotal record #{anecRecs.AnecRecsId} was updated for student {anecRecs.StuName}."
                    });

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnecRecsExists(anecRecs.AnecRecsId)) return NotFound();
                    throw;
                }

                if (studentId.HasValue)
                    return RedirectToAction("Details", "Students", new { id = studentId });

                return RedirectToAction(nameof(Index));
            }

            ViewBag.StudentsID = studentId;
            return View(anecRecs);
        }

        // GET: AnecRecs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anecRecs = await _context.AnecRecs
                .FirstOrDefaultAsync(m => m.AnecRecsId == id);
            if (anecRecs == null)
            {
                return NotFound();
            }

            return View(anecRecs);
        }

        // POST: AnecRecs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? studentId)
        {
            var anecrecs = await _context.AnecRecs.FindAsync(id);
            if (anecrecs != null)
            {
                _context.AnecRecs.Remove(anecrecs);
                await _context.SaveChangesAsync();
            }

            if (studentId.HasValue)
                return RedirectToAction("Details", "Students", new { id = studentId });

            return RedirectToAction(nameof(Index));
        }

        private bool AnecRecsExists(int id)
        {
            return _context.AnecRecs.Any(e => e.AnecRecsId == id);
        }
    }
}
