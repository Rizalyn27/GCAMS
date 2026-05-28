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
                students.IsActive = true;
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

        //Soft Delete
        public async Task<IActionResult> SoftDelete(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            student.IsActive = false;
            _context.Update(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Restore Soft Deleted Student
        public async Task<IActionResult> Restore(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            student.IsActive = true;
            _context.Update(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select an Excel file.";
                return RedirectToAction(nameof(Index));
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Invalid file format. Please upload an .xlsx file.";
                return RedirectToAction(nameof(Index));
            }

            OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("GCAMS Thesis Project");

            var students = new List<Students>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new OfficeOpenXml.ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension?.Rows ?? 0;

            for (int row = 2; row <= rowCount; row++)
            {
                string? Get(int col)
                {
                    var val = worksheet.Cells[row, col].Text?.Trim();
                    return string.IsNullOrWhiteSpace(val) ? null : val;
                }

                var student = new Students
                {
                    StudentId = Get(1) ?? "",
                    StuName = Get(2) ?? "",
                    GradeLevel = Get(3) ?? "",
                    Section = Get(4) ?? "",
                    School = Get(5) ?? "Don Sergio Osmeña Senior Memorial National High School",
                    Birthday = DateTime.TryParse(Get(6), out var bday) ? bday : DateTime.MinValue,
                    Age = int.TryParse(Get(7), out int age) ? age : 0,
                    BirthOrder = Get(8),
                    Address = Get(9) ?? "",
                    ContactNumber = Get(10),
                    Email = Get(11),
                    Gender = Get(12),
                    Nationality = Get(13),
                    Religion = Get(14),
                    StayingWith = Get(15),

                    FatherName = Get(16),
                    FatherAge = int.TryParse(Get(17), out int fAge) ? fAge : (int?)null,
                    FatherEducationalAttainment = Get(18),
                    FatherOccupation = Get(19),
                    FatherContactNumber = Get(20),
                    MotherName = Get(21),
                    MotherAge = int.TryParse(Get(22), out int mAge) ? mAge : (int?)null,
                    MotherEducationalAttainment = Get(23),
                    MotherOccupation = Get(24),
                    MotherContactNumber = Get(25),
                    MonthlyFamilyIncome = Get(26),
                    ParentsRelationshipStatus = Get(27),

                    EmergencyContactPerson = Get(28),
                    EmergencyContactAge = int.TryParse(Get(29), out int ecAge) ? ecAge : (int?)null,
                    EmergencyContactOccupation = Get(30),
                    EmergencyContactNumber = Get(31),
                    EmergencyContactAddress = Get(32),
                    ElementarySchool = Get(33),
                    ElementaryYear = Get(34),
                    ElementaryHonors = Get(35),
                    SecondarySchool = Get(36),
                    SecondaryYear = Get(37),
                    SecondaryHonors = Get(38),

                    Weight = Get(39),
                    Height = Get(40),
                    BloodType = null,
                    Ailments = null,
                    Medication = null,
                    SuicidalAttempts = null,
                    VictimOfAbuse = null,
                    InvolvedWithDrugs = null,
                    MentallyChallengedRelative = null,
                    VisitedPsychiatrist = null,
                    AdditionalNotes = null,
                };

                if (!string.IsNullOrWhiteSpace(student.StuName) && student.Age > 0)
                    students.Add(student);
            }

            try
            {
                await _context.Students.AddRangeAsync(students);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"{students.Count} student(s) imported successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Import failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
