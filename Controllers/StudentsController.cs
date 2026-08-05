using GCAMS.Data;
using GCAMS.Models.CaseNotes;
using GCAMS.Models.Students;
using GCAMS.Models.Users;
using GCAMS.ViewModels;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using GCAMS.Controllers;

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

            var familyId = student.FamilyBackground?.FamilyBackgroundID ?? 0;
            var emergencyId = student.EmergencyContact?.EmergencyContactID ?? 0;

            var vm = new StudentFormViewModel
            {
                Student = student,
                Family = student.FamilyBackground ?? new FamilyBackground(),
                Emergency = student.EmergencyContact ?? new EmergencyContact(),
                Education = student.EducationalBackground ?? new EducationalBackground(),
                Health = student.HealthInformation ?? new HealthInformation(),

                StudentContacts = await _context.StudentContactNumbers
                    .Where(x => x.StudentsID == id)
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                FatherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Label == "Father")
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                MotherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Label == "Mother")
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                EmergencyContacts = await _context.EmergencyContactNumbers
                    .Where(x => x.EmergencyContactID == emergencyId)
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                CaseNotes = await _context.CaseNotes
                    .Where(n => n.StudentsID == student.StudentsID)
                    .OrderByDescending(n => n.SessionDate)
                    .ToListAsync(),

                AnecRecs = await _context.AnecRecs
                    .Where(n => n.StudentsID == student.StudentsID)
                    .OrderByDescending(n => n.AnecRecNo)
                    .ToListAsync()
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
            if (ModelState.IsValid)
            {
                vm.Student.IsActive = true;
                _context.Students.Add(vm.Student);
                await _context.SaveChangesAsync();

                await EnsureStudentAccountAsync(vm.Student.StuID);

                vm.Family.StudentsID = vm.Student.StudentsID;
                vm.Emergency.StudentsID = vm.Student.StudentsID;
                vm.Education.StudentsID = vm.Student.StudentsID;
                vm.Health.StudentsID = vm.Student.StudentsID;

                _context.FamilyBackgrounds.Add(vm.Family);
                _context.EmergencyContacts.Add(vm.Emergency);
                _context.EducationalBackgrounds.Add(vm.Education);
                _context.HealthInformations.Add(vm.Health);

                await _context.SaveChangesAsync();

                foreach (var c in vm.StudentContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.StudentContactNumbers.Add(new StudentContactNumber
                    {
                        StudentsID = vm.Student.StudentsID,
                        Number = c.Number,
                        Label = c.Label
                    });

                foreach (var c in vm.FatherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.FamilyContactNumbers.Add(new FamilyContactNumber
                    {
                        FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                        Number = c.Number,
                        Label = c.Label ?? "Father"
                    });

                foreach (var c in vm.MotherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.FamilyContactNumbers.Add(new FamilyContactNumber
                    {
                        FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                        Number = c.Number,
                        Label = c.Label ?? "Mother"
                    });

                foreach (var c in vm.EmergencyContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                    {
                        EmergencyContactID = vm.Emergency.EmergencyContactID,
                        Number = c.Number,
                        Label = c.Label
                    });

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

            var familyId = student.FamilyBackground?.FamilyBackgroundID ?? 0;
            var emergencyId = student.EmergencyContact?.EmergencyContactID ?? 0;

            var vm = new StudentFormViewModel
            {
                Student = student,
                Family = student.FamilyBackground ?? new FamilyBackground { StudentsID = student.StudentsID },
                Emergency = student.EmergencyContact ?? new EmergencyContact { StudentsID = student.StudentsID },
                Education = student.EducationalBackground ?? new EducationalBackground { StudentsID = student.StudentsID },
                Health = student.HealthInformation ?? new HealthInformation { StudentsID = student.StudentsID },

                StudentContacts = await _context.StudentContactNumbers
                    .Where(x => x.StudentsID == id)
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                FatherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Label == "Father")
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                MotherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Label == "Mother")
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                EmergencyContacts = await _context.EmergencyContactNumbers
                    .Where(x => x.EmergencyContactID == emergencyId)
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),
            };

            return View(vm);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentFormViewModel vm)
        {
            if (id != vm.Student.StudentsID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Grab old StuID before it's overwritten, to sync the linked account after.
                    var oldStuID = await _context.Students
                        .Where(s => s.StudentsID == id)
                        .Select(s => s.StuID)
                        .FirstOrDefaultAsync();

                    _context.Update(vm.Student);

                    if (vm.Family.FamilyBackgroundID == 0) { vm.Family.StudentsID = id; _context.FamilyBackgrounds.Add(vm.Family); }
                    else _context.FamilyBackgrounds.Update(vm.Family);

                    if (vm.Emergency.EmergencyContactID == 0) { vm.Emergency.StudentsID = id; _context.EmergencyContacts.Add(vm.Emergency); }
                    else _context.EmergencyContacts.Update(vm.Emergency);

                    if (vm.Education.EducationalBackgroundID == 0) { vm.Education.StudentsID = id; _context.EducationalBackgrounds.Add(vm.Education); }
                    else _context.EducationalBackgrounds.Update(vm.Education);

                    if (vm.Health.HealthInformationID == 0) { vm.Health.StudentsID = id; _context.HealthInformations.Add(vm.Health); }
                    else _context.HealthInformations.Update(vm.Health);

                    await _context.SaveChangesAsync();

                    var oldStudentContacts = _context.StudentContactNumbers.Where(x => x.StudentsID == id);
                    _context.StudentContactNumbers.RemoveRange(oldStudentContacts);

                    var oldFamilyContacts = _context.FamilyContactNumbers
                        .Where(x => x.FamilyBackgroundID == vm.Family.FamilyBackgroundID);
                    _context.FamilyContactNumbers.RemoveRange(oldFamilyContacts);

                    var oldEmergencyContacts = _context.EmergencyContactNumbers
                        .Where(x => x.EmergencyContactID == vm.Emergency.EmergencyContactID);
                    _context.EmergencyContactNumbers.RemoveRange(oldEmergencyContacts);

                    await _context.SaveChangesAsync();

                    foreach (var c in vm.StudentContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.StudentContactNumbers.Add(new StudentContactNumber
                        {
                            StudentsID = id,
                            Number = c.Number,
                            Label = c.Label
                        });

                    foreach (var c in vm.FatherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                            Number = c.Number,
                            Label = c.Label ?? "Father"
                        });

                    foreach (var c in vm.MotherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                            Number = c.Number,
                            Label = c.Label ?? "Mother"
                        });

                    foreach (var c in vm.EmergencyContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                        {
                            EmergencyContactID = vm.Emergency.EmergencyContactID,
                            Number = c.Number,
                            Label = c.Label
                        });

                    await _context.SaveChangesAsync();

                    await SyncStudentAccountAsync(oldStuID, vm.Student.StuID);
                }
                catch (DbUpdateConcurrencyException)
                {
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

            var student = await _context.Students.FirstOrDefaultAsync(m => m.StudentsID == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Delete/5 (hard delete)
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

        // POST: Students/SoftDelete/5
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

        // POST: Students/Restore/5
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

        // POST: Students/Import (bulk Excel upload)
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

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

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
                        AcademicYear = Get(7) ?? "",
                        BirthOrder = Get(8),
                        Address = Get(9) ?? "",
                        Email = Get(11),
                        Gender = Get(12),
                        Nationality = Get(13),
                        Religion = Get(14),
                        StayingWith = Get(15),
                        IsActive = true
                    };

                    if (string.IsNullOrWhiteSpace(student.StuName) || string.IsNullOrWhiteSpace(student.StuID))
                        continue;

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    await EnsureStudentAccountAsync(student.StuID);

                    if (Get(10) is string stuContact)
                        _context.StudentContactNumbers.Add(new StudentContactNumber
                        {
                            StudentsID = student.StudentsID,
                            Number = stuContact,
                            Label = "Mobile"
                        });

                    var family = new FamilyBackground
                    {
                        StudentsID = student.StudentsID,
                        FatherName = Get(16),
                        FatherAge = int.TryParse(Get(17), out int fAge) ? fAge : null,
                        FatherEducationalAttainment = Get(18),
                        FatherOccupation = Get(19),
                        MotherName = Get(21),
                        MotherAge = int.TryParse(Get(22), out int mAge) ? mAge : null,
                        MotherEducationalAttainment = Get(23),
                        MotherOccupation = Get(24),
                        MonthlyFamilyIncome = Get(26),
                        ParentsRelationshipStatus = Get(27),
                    };
                    _context.FamilyBackgrounds.Add(family);
                    await _context.SaveChangesAsync();

                    if (Get(20) is string fatherContact)
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = family.FamilyBackgroundID,
                            Number = fatherContact,
                            Label = "Father"
                        });

                    if (Get(25) is string motherContact)
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = family.FamilyBackgroundID,
                            Number = motherContact,
                            Label = "Mother"
                        });

                    var emergency = new EmergencyContact
                    {
                        StudentsID = student.StudentsID,
                        EmergencyContactPerson = Get(28),
                        EmergencyContactAge = int.TryParse(Get(29), out int ecAge) ? ecAge : null,
                        EmergencyContactOccupation = Get(30),
                        EmergencyContactAddress = Get(32),
                    };
                    _context.EmergencyContacts.Add(emergency);
                    await _context.SaveChangesAsync();

                    if (Get(31) is string emergencyContact)
                        _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                        {
                            EmergencyContactID = emergency.EmergencyContactID,
                            Number = emergencyContact,
                            Label = "Mobile"
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

        // --- Account helpers ---

        public static bool VerifyHash(string password, byte[] salt, byte[] hash)
        {
            var hashedPassword = UsersController.HashPassword(password, salt);
            return hashedPassword.SequenceEqual(hash);
        }

        private async Task EnsureStudentAccountAsync(string stuID)
        {
            if (string.IsNullOrWhiteSpace(stuID)) return;

            bool exists = await _context.Users.AnyAsync(u => u.Username == stuID);
            if (exists) return;

            byte[] salt = UsersController.CreateSalt();
            byte[] hash = UsersController.HashPassword(stuID, salt);

            var account = new Users
            {
                Username = stuID,
                Password = Convert.ToBase64String(hash),
                Salt = Convert.ToBase64String(salt),
                Role = "Student",
                PasswordChange = false,
            };

            _context.Users.Add(account);
            await _context.SaveChangesAsync();
        }

        // Keeps Users.Username in sync if StuID changes on Edit; creates the account if it's missing.
        private async Task SyncStudentAccountAsync(string oldStuID, string newStuID)
        {
            if (string.IsNullOrWhiteSpace(newStuID)) return;

            if (string.IsNullOrWhiteSpace(oldStuID))
            {
                await EnsureStudentAccountAsync(newStuID);
                return;
            }

            if (string.Equals(oldStuID, newStuID, StringComparison.Ordinal)) return;

            var account = await _context.Users.FirstOrDefaultAsync(u => u.Username == oldStuID);
            if (account == null)
            {
                await EnsureStudentAccountAsync(newStuID);
                return;
            }

            account.Username = newStuID;
            await _context.SaveChangesAsync();
        }
    }
}