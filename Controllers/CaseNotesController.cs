
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

    // GET: CASENOTESS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.CaseNotes.ToListAsync());
    }

    // GET: CASENOTESS/Details/5
    public async Task<IActionResult> Details(int? casenoteid)
    {
        if (casenoteid == null)
        {
            return NotFound();
        }

        var casenotes = await _context.CaseNotes
            .FirstOrDefaultAsync(m => m.CasenoteId == casenoteid);
        if (casenotes == null)
        {
            return NotFound();
        }

        return View(casenotes);
    }

    // GET: CASENOTESS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CASENOTESS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CasenoteId,FullName,SessionNo,AppointmentDate,SessionTopics,SessionRelevance,GoalPlan,Interventions,Observations,CounselProgess,BehaviorStatus,Homework,StrengthsChallenges,SpecificGoal,OverallGoal")] CaseNotes casenotes)
    {
        if (ModelState.IsValid)
        {
            _context.Add(casenotes);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(casenotes);
    }

    // GET: CASENOTESS/Edit/5
    public async Task<IActionResult> Edit(int? casenoteid)
    {
        if (casenoteid == null)
        {
            return NotFound();
        }

        var casenotes = await _context.CaseNotes.FindAsync(casenoteid);
        if (casenotes == null)
        {
            return NotFound();
        }
        return View(casenotes);
    }

    // POST: CASENOTESS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? casenoteid, [Bind("CasenoteId,FullName,SessionNo,AppointmentDate,SessionTopics,SessionRelevance,GoalPlan,Interventions,Observations,CounselProgess,BehaviorStatus,Homework,StrengthsChallenges,SpecificGoal,OverallGoal")] CaseNotes casenotes)
    {
        if (casenoteid != casenotes.CasenoteId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(casenotes);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CaseNotesExists(casenotes.CasenoteId))
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
        return View(casenotes);
    }

    // GET: CASENOTESS/Delete/5
    public async Task<IActionResult> Delete(int? casenoteid)
    {
        if (casenoteid == null)
        {
            return NotFound();
        }

        var casenotes = await _context.CaseNotes
            .FirstOrDefaultAsync(m => m.CasenoteId == casenoteid);
        if (casenotes == null)
        {
            return NotFound();
        }

        return View(casenotes);
    }

    // POST: CASENOTESS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? casenoteid)
    {
        var casenotes = await _context.CaseNotes.FindAsync(casenoteid);
        if (casenotes != null)
        {
            _context.CaseNotes.Remove(casenotes);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CaseNotesExists(int? casenoteid)
    {
        return _context.CaseNotes.Any(e => e.CasenoteId == casenoteid);
    }
}
