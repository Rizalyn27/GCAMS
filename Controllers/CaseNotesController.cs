using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GCAMS.Models.CaseNotes;
using GCAMS.Data;

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

        // Fallback: resolve the student id from FullName if it wasn't passed in
        ViewBag.StudentsID = studentId
            ?? (await _context.Students.FirstOrDefaultAsync(s => s.StuName == casenotes.FullName))?.StudentsID;

        return View(casenotes);
    }

    // GET: CaseNotes/Create?fullName=Juan Dela Cruz
    public IActionResult Create(string? fullName, int? studentId)
    {
        var casenotes = new CaseNotes();
        if (!string.IsNullOrWhiteSpace(fullName))
            casenotes.FullName = fullName;

        ViewBag.StudentsID = studentId;
        return View(casenotes);
    }

    // POST: CaseNotes/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    [Bind("CasenoteId,FullName,SessionNo,SessionDate,SessionRelevance,GoalPlan,Observations,CounselProgess,BehaviorStatus,Homework,StrengthsChallenges,SpecificGoal,OverallGoal")] CaseNotes casenotes,
    string[]? selectedSessionTopics,
    string[]? selectedInterventions,
    int? studentId)
    {
        casenotes.SessionTopics = selectedSessionTopics != null ? string.Join(", ", selectedSessionTopics) : null;
        casenotes.Interventions = selectedInterventions != null ? string.Join(", ", selectedInterventions) : null;

        ModelState.Remove("SessionTopics");
        ModelState.Remove("Interventions");

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

        ViewBag.StudentsID = studentId
            ?? (await _context.Students.FirstOrDefaultAsync(s => s.StuName == casenotes.FullName))?.StudentsID;

        return View(casenotes);
    }

    // POST: CaseNotes/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
     int? id,
     [Bind("CasenoteId,FullName,SessionNo,SessionDate,SessionRelevance,GoalPlan,Observations,CounselProgess,BehaviorStatus,Homework,StrengthsChallenges,SpecificGoal,OverallGoal")] CaseNotes casenotes,
     string[]? sessionTopics,
    string[]? interventions, 
    int? studentId)
    {
        casenotes.SessionTopics = sessionTopics != null ? string.Join(", ", sessionTopics) : null;
        casenotes.Interventions = interventions != null ? string.Join(", ", interventions) : null;

        ModelState.Remove("SessionTopics");
        ModelState.Remove("Interventions");
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
        if (id == null)
        {
            return NotFound();
        }

        var casenotes = await _context.CaseNotes
            .FirstOrDefaultAsync(m => m.CasenoteId == id);
        if (casenotes == null)
        {
            return NotFound();
        }

        return View(casenotes);
    }

    // POST: CaseNotes/Delete/5
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