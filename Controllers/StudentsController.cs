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


        //Import Students from Excel
        //public IActionResult Import()
        //{
        //    return View();
        //}

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
                    FName = Get(1) ?? "",
                    LName = Get(2) ?? "",
                    MName = Get(3) ?? "",
                    GradeLevel = Get(4) ?? "",
                    Section = Get(5) ?? "",
                    School = Get(6) ?? "Don Sergio Osmeña Senior Memorial National High School",
                    Birthday = DateTime.TryParse(Get(7), out var bday) ? bday : DateTime.MinValue,
                    Age = int.TryParse(Get(8), out int age) ? age : 0,
                    Address = Get(10) ?? "",

                    BirthOrder = Get(9),
                    ContactNumber = Get(11),
                    Email = Get(12),
                    Gender = Get(13),
                    Nationality = Get(14),
                    Religion = Get(15),
                    StayingWith = Get(16),

                    FatherName = Get(17),
                    FatherAge = int.TryParse(Get(18), out int fAge) ? fAge : (int?)null,
                    FatherEducationalAttainment = Get(19),
                    FatherOccupation = Get(20),
                    FatherContactNumber = Get(21),
                    MotherName = Get(22),
                    MotherAge = int.TryParse(Get(23), out int mAge) ? mAge : (int?)null,
                    MotherEducationalAttainment = Get(24),
                    MotherOccupation = Get(25),
                    MotherContactNumber = Get(26),
                    MonthlyFamilyIncome = Get(27),
                    ParentsRelationshipStatus = Get(28),

                    EmergencyContactPerson = Get(29),
                    EmergencyContactAge = int.TryParse(Get(30), out int ecAge) ? ecAge : (int?)null,
                    EmergencyContactOccupation = Get(31),
                    EmergencyContactNumber = Get(32),
                    EmergencyContactAddress = Get(33),

                    ElementarySchool = Get(34),
                    ElementaryYear = Get(35),
                    ElementaryHonors = Get(36),
                    SecondarySchool = Get(37),
                    SecondaryYear = Get(38),
                    SecondaryHonors = Get(39),

                    Height = null,
                    Weight = null,
                    BloodType = null,
                    Ailments = null,
                    Medication = null,
                    SuicidalAttempts = null,
                    VictimOfAbuse = null,
                    InvolvedWithDrugs = null,
                    MentallyChallengedRelative = null,
                    VisitedPsychiatrist = null,
                    VisitedPsychiatristReason = null,
                };

                if (!string.IsNullOrWhiteSpace(student.FName) && student.Age > 0)
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
