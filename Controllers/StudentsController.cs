using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GCAMS.Data;
using GCAMS.Models;

namespace GCAMS.Controllers
{
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var students = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (students == null)
            {
                return NotFound();
            }

            return View(students);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FName,LName,MName,GradeLevel,Section,School,Birthday,Age,BirthOrder,Address,ContactNumber,Email,Gender,Nationality,Religion,StayingWith,FatherName,FatherAge,FatherEducationalAttainment,FatherOccupation,FatherContactNumber,MotherName,MotherAge,MotherEducationalAttainment,MotherOccupation,MotherContactNumber,MonthlyFamilyIncome,ParentsRelationshipStatus,EmergencyContactPerson,EmergencyContactAge,EmergencyContactOccupation,EmergencyContactNumber,EmergencyContactAddress,ElementarySchool,ElementaryYear,ElementaryHonors,SecondarySchool,SecondaryYear,SecondaryHonors,Height,Weight,BloodType,Ailments,Medication,SuicidalAttempts,VictimOfAbuse,InvolvedWithDrugs,MentallyChallengedRelative,VisitedPsychiatrist,VisitedPsychiatristReason")] Students students)
        {
            if (ModelState.IsValid)
            {
                _context.Add(students);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(students);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var students = await _context.Students.FindAsync(id);
            if (students == null)
            {
                return NotFound();
            }
            return View(students);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FName,LName,MName,GradeLevel,Section,School,Birthday,Age,BirthOrder,Address,ContactNumber,Email,Gender,Nationality,Religion,StayingWith,FatherName,FatherAge,FatherEducationalAttainment,FatherOccupation,FatherContactNumber,MotherName,MotherAge,MotherEducationalAttainment,MotherOccupation,MotherContactNumber,MonthlyFamilyIncome,ParentsRelationshipStatus,EmergencyContactPerson,EmergencyContactAge,EmergencyContactOccupation,EmergencyContactNumber,EmergencyContactAddress,ElementarySchool,ElementaryYear,ElementaryHonors,SecondarySchool,SecondaryYear,SecondaryHonors,Height,Weight,BloodType,Ailments,Medication,SuicidalAttempts,VictimOfAbuse,InvolvedWithDrugs,MentallyChallengedRelative,VisitedPsychiatrist,VisitedPsychiatristReason")] Students students)
        {
            if (id != students.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(students);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentsExists(students.Id))
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
            return View(students);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var students = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (students == null)
            {
                return NotFound();
            }

            return View(students);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var students = await _context.Students.FindAsync(id);
            if (students != null)
            {
                _context.Students.Remove(students);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentsExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
