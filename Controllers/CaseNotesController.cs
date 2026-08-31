using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GCAMS.Models.CaseNotes;
using GCAMS.Models.Appointment;
using GCAMS.Data;
using System.Threading.Tasks;

[Authorize]
public class CaseNotesController : Controller
{
    private readonly AppDbContext _context;


    public CaseNotesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: CaseNotes
    public async Task<IActionResult> Index()
    {
        return View(await _context.CaseNotes.ToListAsync());
    }

    // GET: CaseNotes/Details/5
    public async Task<IActionResult> Details(int? id, int? studentId)
    {
        if (id == null) return NotFound();

        var casenotes = await _context.CaseNotes
            .Include(c => c.Counselor)
            .FirstOrDefaultAsync(m => m.CasenoteId == id);
        if (casenotes == null) return NotFound();

        ViewBag.StudentsID = studentId ?? casenotes.StudentsID;
        return View(casenotes);
    }

    // GET: CaseNotes/Create?studentId=5
    public async Task<IActionResult> Create(int? studentId)
    {
        var casenotes = new CaseNotes();

        if (studentId.HasValue)
        {
            casenotes.StudentsID = studentId.Value;

            var student = await _context.Students.FindAsync(studentId.Value);
            if (student != null) casenotes.FullName = student.StuName;

            casenotes.SessionNo = await _context.CaseNotes
                .CountAsync(n => n.StudentsID == studentId.Value) + 1;
        }

        ModelState.Remove("SessionNo");
        ViewBag.StudentsID = studentId;
        return View(casenotes);
    }

    private static readonly int[] AllowedHours = { 8, 9, 10, 11, 13, 14, 15, 16 };
    private static bool IsWithinWorkingHours(DateTime dt) => AllowedHours.Contains(dt.Hour);

    // POST: CaseNotes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    [Bind("CasenoteId,StudentsID,FullName,SessionNo,SessionDate,SessionTopics,SessionRelevance,GoalPlan,Interventions,Observations,CounselProgess,BehaviorStatus,Homework,StrengthsChallenges,SpecificGoal,FollowUpDate")] CaseNotes casenotes, int? studentId)
    {
        if (studentId.HasValue) casenotes.StudentsID = studentId.Value;

        if (ModelState.IsValid)
        {
            casenotes.CounselorID = await GetCurrentCounselorIdAsync();

            _context.Add(casenotes);
            await _context.SaveChangesAsync();

            // Moved above the redirect — now actually runs regardless of studentId
            if (casenotes.FollowUpDate.HasValue)
            {
                var followUpDate = casenotes.FollowUpDate.Value.Date
                    .AddHours(casenotes.FollowUpDate.Value.Hour);

                if (!IsWithinWorkingHours(followUpDate))
                {
                    TempData["Error"] = "Follow-up time must be between 8–11 AM or 1–4 PM. The case note was saved, but no follow-up appointment was booked.";
                }
                else
                {
                    var slotTaken = await _context.Appointments.AnyAsync(a =>
                        a.AppointmentDate == followUpDate &&
                        a.Status != "Cancelled" &&
                        a.Status != "Missed");

                    if (slotTaken)
                    {
                        TempData["Error"] = "That follow-up time slot is already booked. The case note was saved, but no follow-up appointment was created — please schedule one manually.";
                    }
                    else
                    {
                        var student = await _context.Students.FindAsync(casenotes.StudentsID);

                        _context.Appointments.Add(new Appointments
                        {
                            StudentsID = casenotes.StudentsID,
                            CounselorID = casenotes.CounselorID,
                            FullName = student?.StuName ?? casenotes.FullName,
                            Email = student?.Email ?? "",
                            AppointmentDate = followUpDate,
                            AppointmentType = "Follow-up",
                            Status = "Confirmed", // counselor-initiated, already committed
                            CreatedAt = DateTime.Now,
                            Notes = $"Follow-up from Case Note #{casenotes.CasenoteId}"
                        });

                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (studentId.HasValue)
                return RedirectToAction("Details", "Students", new { id = studentId });

            return RedirectToAction(nameof(Index));
        }

        ViewBag.StudentsID = studentId;
        return View(casenotes);
    }

    // GET: CaseNotes/Edit/5
    public async Task<IActionResult> Edit(int? id, int? studentId)
    {
        if (id == null) return NotFound();

        var casenotes = await _context.CaseNotes.FindAsync(id);
        if (casenotes == null) return NotFound();

        ViewBag.StudentsID = studentId ?? casenotes.StudentsID;
        return View(casenotes);
    }

    // POST: CaseNotes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id,
        [Bind("CasenoteId,StudentsID,FullName,SessionNo,SessionDate,SessionTopics,SessionRelevance,GoalPlan,Interventions,Observations,CounselProgess,BehaviorStatus,Homework,StrengthsChallenges,SpecificGoal")] CaseNotes casenotes, int? studentId)
    {
        if (id != casenotes.CasenoteId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Keep the original author on edit — don't overwrite CounselorID with
                // the current editor, since it wasn't part of the bound fields anyway
                // (EF will leave it untouched since it's not on the tracked entity here).
                var existingCounselorId = await _context.CaseNotes
                    .Where(c => c.CasenoteId == id)
                    .Select(c => c.CounselorID)
                    .FirstOrDefaultAsync();
                casenotes.CounselorID = existingCounselorId;

                _context.Update(casenotes);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CaseNotesExists(casenotes.CasenoteId)) return NotFound();
                throw;
            }

            if (studentId.HasValue)
                return RedirectToAction("Details", "Students", new { id = studentId });

            return RedirectToAction(nameof(Index));
        }

        ViewBag.StudentsID = studentId;
        return View(casenotes);
    }

    // GET: CaseNotes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var casenotes = await _context.CaseNotes.FirstOrDefaultAsync(m => m.CasenoteId == id);
        if (casenotes == null) return NotFound();

        return View(casenotes);
    }

    // POST: CaseNotes/Delete/5 (hard delete — no undo)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id, int? studentId)
    {
        var casenotes = await _context.CaseNotes.FindAsync(id);
        if (casenotes != null)
        {
            _context.CaseNotes.Remove(casenotes);
            await _context.SaveChangesAsync();
        }

        if (studentId.HasValue)
            return RedirectToAction("Details", "Students", new { id = studentId });

        return RedirectToAction(nameof(Index));
    }

    private bool CaseNotesExists(int? id)
    {
        return _context.CaseNotes.Any(e => e.CasenoteId == id);
    }

    // Resolves the logged-in counselor's ID from their email (== their Username).
    private async Task<int?> GetCurrentCounselorIdAsync()
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email)) return null;

        return await _context.Counselors
            .Where(c => c.EmailAddress == email)
            .Select(c => (int?)c.CounselorID)
            .FirstOrDefaultAsync();
    }
}