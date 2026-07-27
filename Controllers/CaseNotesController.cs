using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GCAMS.Models.CaseNotes;
using GCAMS.Data;
using System.Threading.Tasks;

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

        var casenotes = await _context.CaseNotes.FirstOrDefaultAsync(m => m.CasenoteId == id);
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

    // POST: CaseNotes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("CasenoteId,StudentsID,FullName,SessionNo,SessionDate,SessionTopics,SessionRelevance,GoalPlan,Interventions,Observations,CounselProgess,BehaviorStatus,Homework,StrengthsChallenges,SpecificGoal")] CaseNotes casenotes, int? studentId)
    {
        if (studentId.HasValue) casenotes.StudentsID = studentId.Value;

        if (ModelState.IsValid)
        {
            _context.Add(casenotes);
            await _context.SaveChangesAsync();

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
}