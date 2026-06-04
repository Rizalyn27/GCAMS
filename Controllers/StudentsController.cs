using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GCAMS.Data;
using GCAMS.Models.Students;
using GCAMS.ViewModels;
using System.Globalization;

namespace GCAMS.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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
            try
            {
                var students = await _context.Students.ToListAsync();
                return View(students);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.FamilyBackground)
                .Include(s => s.EmergencyContact)
                .Include(s => s.EducationalBackground)
                .Include(s => s.HealthInformation)
                .FirstOrDefaultAsync(m => m.StudentsID == id);

            if (student == null) return NotFound();


            var vm = new StudentFormViewModel
            {
                Student = student,
                Family = student.FamilyBackground ?? new FamilyBackground(),
                Emergency = student.EmergencyContact ?? new EmergencyContact(),
                Education = student.EducationalBackground ?? new EducationalBackground(),
                Health = student.HealthInformation ?? new HealthInformation()
            };

            return View(vm);
        }

        // GET: Students/Create

        public IActionResult Create()
        {
            return View(new StudentFormViewModel());
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentFormViewModel vm)
        {
            foreach (var error in ModelState)
            {
                var key = error.Key;
                var errors = error.Value.Errors;
            }
            if (ModelState.IsValid)
            {
                // 1. Save student first to get StudentsID (PK)
                vm.Student.IsActive = true;
                _context.Students.Add(vm.Student);
                await _context.SaveChangesAsync();

                // 2. Link and save related entities
                vm.Family.StudentsID = vm.Student.StudentsID;
                vm.Emergency.StudentsID = vm.Student.StudentsID;
                vm.Education.StudentsID = vm.Student.StudentsID;
                vm.Health.StudentsID = vm.Student.StudentsID;

                _context.FamilyBackgrounds.Add(vm.Family);
                _context.EmergencyContacts.Add(vm.Emergency);
                _context.EducationalBackgrounds.Add(vm.Education);
                _context.HealthInformations.Add(vm.Health);


                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.FamilyBackground)
                .Include(s => s.EmergencyContact)
                .Include(s => s.EducationalBackground)
                .Include(s => s.HealthInformation)
                .FirstOrDefaultAsync(s => s.StudentsID == id);

            if (student == null) return NotFound();

            var vm = new StudentFormViewModel
            {
                Student = student,
                Family = student.FamilyBackground ?? new FamilyBackground { StudentsID = student.StudentsID },
                Emergency = student.EmergencyContact ?? new EmergencyContact { StudentsID = student.StudentsID },
                Education = student.EducationalBackground ?? new EducationalBackground { StudentsID = student.StudentsID },
                Health = student.HealthInformation ?? new HealthInformation { StudentsID = student.StudentsID }
            };

            return View(vm);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentFormViewModel vm)
        {
            if (id != vm.Student.StudentsID) return NotFound();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(errors);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vm.Student);

                    //update related entities
                    //if entites still have nothing add new content

                    if (vm.Family.FamilyBackgroundID == 0) { vm.Family.StudentsID = id; _context.FamilyBackgrounds.Add(vm.Family); }
                    else _context.FamilyBackgrounds.Update(vm.Family);

                    if (vm.Emergency.EmergencyContactID == 0) { vm.Emergency.StudentsID = id; _context.EmergencyContacts.Add(vm.Emergency); }
                    else _context.EmergencyContacts.Update(vm.Emergency);

                    if (vm.Education.EducationalBackgroundID == 0) { vm.Education.StudentsID = id; _context.EducationalBackgrounds.Add(vm.Education); }
                    else _context.EducationalBackgrounds.Update(vm.Education);

                    if (vm.Health.HealthInformationID == 0) { vm.Health.StudentsID = id; _context.HealthInformations.Add(vm.Health); }
                    else _context.HealthInformations.Update(vm.Health);


                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    //if student doesnt exits, return not found error
                    if (!StudentsExists(vm.Student.StudentsID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.StudentsID == id);

            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
                _context.Students.Remove(student);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentsExists(int id)
        {
            return _context.Students.Any(e => e.StudentsID == id);
        }

        // Soft Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.IsActive = false;
            _context.Update(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Restore/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.IsActive = true;
            _context.Update(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            

            try
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


                //save excel file to stream temporarilly
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                //open excel file, sheet 0
                using var package = new OfficeOpenXml.ExcelPackage(stream);
                if (package.Workbook.Worksheets.Count == 0)
                {
                    TempData["Error"] = "The Excel file has no worksheets.";
                    return RedirectToAction(nameof(Index));
                }

                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int successCount = 0;


                for (int row = 2; row <= rowCount; row++)
                {
                    string? Get(int col)
                    {
                        var val = worksheet.Cells[row, col].Text?.Trim();
                        return string.IsNullOrWhiteSpace(val) ? null : val;
                    }

                    var student = new Students
                    {
                        StuID = Get(1) ?? "",
                        StuName = Get(2) ?? "",
                        GradeLevel = Get(3) ?? "",
                        Section = Get(4) ?? "",
                        School = Get(5) ?? "Don Sergio Osmeña Senior Memorial National High School",
                        Birthday = DateTime.TryParse(Get(6), out var bday) ? bday : null,
                        Age = int.TryParse(Get(7), out int age) ? age : 0,
                        BirthOrder = Get(8),
                        Address = Get(9) ?? "",
                        ContactNumber = Get(10),
                        Email = Get(11),
                        Gender = Get(12),
                        Nationality = Get(13),
                        Religion = Get(14),
                        StayingWith = Get(15),
                        IsActive = true
                    };

                    if (string.IsNullOrWhiteSpace(student.StuName) || string.IsNullOrWhiteSpace(student.StuID))
                        continue;

                    // Save student first to get StudentsID
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    // Save related entities linked to the student
                    _context.FamilyBackgrounds.Add(new FamilyBackground
                    {
                        StudentsID = student.StudentsID,
                        FatherName = Get(16),
                        FatherAge = int.TryParse(Get(17), out int fAge) ? fAge : null,
                        FatherEducationalAttainment = Get(18),
                        FatherOccupation = Get(19),
                        FatherContactNumber = Get(20),
                        MotherName = Get(21),
                        MotherAge = int.TryParse(Get(22), out int mAge) ? mAge : null,
                        MotherEducationalAttainment = Get(23),
                        MotherOccupation = Get(24),
                        MotherContactNumber = Get(25),
                        MonthlyFamilyIncome = Get(26),
                        ParentsRelationshipStatus = Get(27),
                    });

                    _context.EmergencyContacts.Add(new EmergencyContact
                    {
                        StudentsID = student.StudentsID,
                        EmergencyContactPerson = Get(28),
                        EmergencyContactAge = int.TryParse(Get(29), out int ecAge) ? ecAge : null,
                        EmergencyContactOccupation = Get(30),
                        EmergencyContactNumber = Get(31),
                        EmergencyContactAddress = Get(32),
                    });

                    _context.EducationalBackgrounds.Add(new EducationalBackground
                    {
                        StudentsID = student.StudentsID,
                        ElementarySchool = Get(33),
                        ElementaryYear = Get(34),
                        ElementaryHonors = Get(35),
                        SecondarySchool = Get(36),
                        SecondaryYear = Get(37),
                        SecondaryHonors = Get(38),
                    });

                    _context.HealthInformations.Add(new HealthInformation
                    {
                        StudentsID = student.StudentsID,
                        Weight = Get(39),
                        Height = Get(40),
                    });

                    await _context.SaveChangesAsync();
                    successCount++;
                }

                TempData["Success"] = $"{successCount} student(s) imported successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(Index));
            }

        }
    }
}