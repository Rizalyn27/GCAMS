using GCAMS.Controllers;
using GCAMS.Data;
using GCAMS.Models.ActivityLogs;
using GCAMS.Models.CaseNotes;
using GCAMS.Models.Students;
using GCAMS.Models.Users;
using GCAMS.ViewModels;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
                    .Select(x => new ContactEntry { Number = x.Number })
                    .ToListAsync(),

                FatherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Owner == "Father")
                    .Select(x => new ContactEntry { Number = x.Number })
                    .ToListAsync(),

                                MotherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Owner == "Mother")
                    .Select(x => new ContactEntry { Number = x.Number })
                    .ToListAsync(),

                EmergencyContacts = await _context.EmergencyContactNumbers
                    .Where(x => x.EmergencyContactID == emergencyId)
                    .Select(x => new ContactEntry { Number = x.Number })
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
                        Number = StudentRules.NormalizeMobile(c.Number)
                    });

                foreach (var c in vm.FatherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.FamilyContactNumbers.Add(new FamilyContactNumber
                    {
                        FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                        Number = StudentRules.NormalizeMobile(c.Number),
                        Owner = "Father"
                    });

                foreach (var c in vm.MotherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.FamilyContactNumbers.Add(new FamilyContactNumber
                    {
                        FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                        Number = StudentRules.NormalizeMobile(c.Number),
                        Owner = "Mother"
                    });

                foreach (var c in vm.EmergencyContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                    {
                        EmergencyContactID = vm.Emergency.EmergencyContactID,
                        Number = StudentRules.NormalizeMobile(c.Number)
                    });

                await _context.SaveChangesAsync();

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Who = User.Identity?.Name ?? "Unknown",
                    Date = DateTime.Now,
                    ActivityAction = ActivityAction.StudentAdded.ToString(),
                    Details = $"Student {vm.Student.StuName} ({vm.Student.StuID}) was added."
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
                    .Select(x => new ContactEntry { Number = x.Number })
                    .ToListAsync(),

                FatherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Owner == "Father")
                    .Select(x => new ContactEntry { Number = x.Number })
                    .ToListAsync(),

                MotherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Owner == "Mother")
                    .Select(x => new ContactEntry { Number = x.Number })
                    .ToListAsync(),

                EmergencyContacts = await _context.EmergencyContactNumbers
                    .Where(x => x.EmergencyContactID == emergencyId)
                    .Select(x => new ContactEntry { Number = x.Number })
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
                            Number = StudentRules.NormalizeMobile(c.Number)
                        });

                    foreach (var c in vm.FatherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                            Number = StudentRules.NormalizeMobile(c.Number),
                            Owner = "Father"
                        });

                    foreach (var c in vm.MotherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                            Number = StudentRules.NormalizeMobile(c.Number),
                            Owner = "Mother"
                        });

                    foreach (var c in vm.EmergencyContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                        {
                            EmergencyContactID = vm.Emergency.EmergencyContactID,
                            Number = StudentRules.NormalizeMobile(c.Number)
                        });

                    await _context.SaveChangesAsync();

                    await SyncStudentAccountAsync(oldStuID, vm.Student.StuID);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentsExists(vm.Student.StudentsID)) return NotFound();
                    else throw;
                }

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Who = User.Identity?.Name ?? "Unknown",
                    Date = DateTime.Now,
                    ActivityAction = ActivityAction.StudentUpdated.ToString(),
                    Details = $"Student {vm.Student.StuName} ({vm.Student.StuID}) was updated."
                });
                await _context.SaveChangesAsync();

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

            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = User.Identity?.Name ?? "Unknown",
                Date = DateTime.Now,
                ActivityAction = ActivityAction.StudentSetInactive.ToString(),
                Details = $"Student {student.StuName} ({student.StuID}) was archived."
            });

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

            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = User.Identity?.Name ?? "Unknown",
                Date = DateTime.Now,
                ActivityAction = ActivityAction.StudentSetActive.ToString(),
                Details = $"Student {student.StuName} ({student.StuID}) was restored."
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Students/Import (bulk Excel upload)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            List<Students> Students = new List<Students>();
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
                int skippedCount = 0;

                var ExistingStudent = await _context.Students
                    .Select(s => new { s.StuID, s.StuName })
                    .ToListAsync();

                var seenKeys = new HashSet<string>(
                    ExistingStudent.Select(s => $"{s.StuID}|{s.StuName}"),
                    StringComparer.OrdinalIgnoreCase);


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

                    string key = $"{student.StuID}|{student.StuName}";
                    if (seenKeys.Contains(key))
                    {
                        skippedCount++;
                        continue;
                    }
                    seenKeys.Add(key);

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    await EnsureStudentAccountAsync(student.StuID);

                    if (Get(10) is string stuContact)
                        _context.StudentContactNumbers.Add(new StudentContactNumber
                        {
                            StudentsID = student.StudentsID,
                            Number = stuContact
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
                            Number = fatherContact
                        });

                    if (Get(25) is string motherContact)
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = family.FamilyBackgroundID,
                            Number = motherContact
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
                            Number = emergencyContact
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
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Who = User.Identity?.Name ?? "Unknown",
                    Date = DateTime.Now,
                    ActivityAction = ActivityAction.StudentAdded.ToString(),
                    Details = $"Imported {successCount} student(s) from Excel." +
              (skippedCount > 0 ? $" {skippedCount} duplicate(s) skipped." : "")
                });
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{successCount} student(s) imported successfully." + (skippedCount > 0 ? $" {skippedCount} duplicate(s) skipped." : "");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        //Export For Edit
        // GET: Students/Export
        // Produces an .xlsx in the exact same 40-column layout Import/BulkUpdatePreview
        // expect, so "Export, edit in Excel, Bulk Update" is a real round-trip.
        public async Task<IActionResult> Export()
        {
            var students = await _context.Students
                .Include(s => s.FamilyBackground)
                .Include(s => s.EmergencyContact)
                .Include(s => s.EducationalBackground)
                .Include(s => s.HealthInformation)
                .OrderBy(s => s.StuName)
                .ToListAsync();

            var studentIds = students.Select(s => s.StudentsID).ToList();
            var familyIds = students.Where(s => s.FamilyBackground != null)
                .Select(s => s.FamilyBackground!.FamilyBackgroundID).ToList();
            var emergencyIds = students.Where(s => s.EmergencyContact != null)
                .Select(s => s.EmergencyContact!.EmergencyContactID).ToList();

            // One number per role, matching what Import/BulkUpdatePreview read from a single cell.
            var studentContacts = await _context.StudentContactNumbers
                .Where(x => studentIds.Contains(x.StudentsID))
                .GroupBy(x => x.StudentsID)
                .ToDictionaryAsync(g => g.Key, g => g.First().Number);

            var familyContacts = await _context.FamilyContactNumbers
                .Where(x => familyIds.Contains(x.FamilyBackgroundID))
                .ToListAsync();
            var fatherContacts = familyContacts.Where(x => x.Owner == "Father")
                .GroupBy(x => x.FamilyBackgroundID)
                .ToDictionary(g => g.Key, g => g.First().Number);
            var motherContacts = familyContacts.Where(x => x.Owner == "Mother")
                .GroupBy(x => x.FamilyBackgroundID)
                .ToDictionary(g => g.Key, g => g.First().Number);

            var emergencyContacts = await _context.EmergencyContactNumbers
                .Where(x => emergencyIds.Contains(x.EmergencyContactID))
                .GroupBy(x => x.EmergencyContactID)
                .ToDictionaryAsync(g => g.Key, g => g.First().Number);

            using var package = new OfficeOpenXml.ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Students");

            string[] headers =
            {
        "StuID", "StuName", "GradeLevel", "Section", "School", "Birthday", "AcademicYear",
        "BirthOrder", "Address", "StudentContact", "Email", "Gender", "Nationality", "Religion",
        "StayingWith", "FatherName", "FatherAge", "FatherEducationalAttainment", "FatherOccupation",
        "FatherContact", "MotherName", "MotherAge", "MotherEducationalAttainment", "MotherOccupation",
        "MotherContact", "MonthlyFamilyIncome", "ParentsRelationshipStatus", "EmergencyContactPerson",
        "EmergencyContactAge", "EmergencyContactOccupation", "EmergencyContactNumber",
        "EmergencyContactAddress", "ElementarySchool", "ElementaryYear", "ElementaryHonors",
        "SecondarySchool", "SecondaryYear", "SecondaryHonors", "Weight", "Height"
    };

            for (int i = 0; i < headers.Length; i++)
                ws.Cells[1, i + 1].Value = headers[i];

            int row = 2;
            foreach (var s in students)
            {
                var fam = s.FamilyBackground;
                var emer = s.EmergencyContact;
                var edu = s.EducationalBackground;
                var health = s.HealthInformation;

                var famId = fam?.FamilyBackgroundID ?? 0;
                var emerId = emer?.EmergencyContactID ?? 0;

                int col = 1;
                ws.Cells[row, col++].Value = s.StuID;
                ws.Cells[row, col++].Value = s.StuName;
                ws.Cells[row, col++].Value = s.GradeLevel;
                ws.Cells[row, col++].Value = s.Section;
                ws.Cells[row, col++].Value = s.School;
                ws.Cells[row, col].Value = s.Birthday;
                ws.Cells[row, col++].Style.Numberformat.Format = "yyyy-mm-dd";
                ws.Cells[row, col++].Value = s.AcademicYear;
                ws.Cells[row, col++].Value = s.BirthOrder;
                ws.Cells[row, col++].Value = s.Address;
                ws.Cells[row, col++].Value = studentContacts.GetValueOrDefault(s.StudentsID);
                ws.Cells[row, col++].Value = s.Email;
                ws.Cells[row, col++].Value = s.Gender;
                ws.Cells[row, col++].Value = s.Nationality;
                ws.Cells[row, col++].Value = s.Religion;
                ws.Cells[row, col++].Value = s.StayingWith;
                ws.Cells[row, col++].Value = fam?.FatherName;
                ws.Cells[row, col++].Value = fam?.FatherAge;
                ws.Cells[row, col++].Value = fam?.FatherEducationalAttainment;
                ws.Cells[row, col++].Value = fam?.FatherOccupation;
                ws.Cells[row, col++].Value = fatherContacts.GetValueOrDefault(famId);
                ws.Cells[row, col++].Value = fam?.MotherName;
                ws.Cells[row, col++].Value = fam?.MotherAge;
                ws.Cells[row, col++].Value = fam?.MotherEducationalAttainment;
                ws.Cells[row, col++].Value = fam?.MotherOccupation;
                ws.Cells[row, col++].Value = motherContacts.GetValueOrDefault(famId);
                ws.Cells[row, col++].Value = fam?.MonthlyFamilyIncome;
                ws.Cells[row, col++].Value = fam?.ParentsRelationshipStatus;
                ws.Cells[row, col++].Value = emer?.EmergencyContactPerson;
                ws.Cells[row, col++].Value = emer?.EmergencyContactAge;
                ws.Cells[row, col++].Value = emer?.EmergencyContactOccupation;
                ws.Cells[row, col++].Value = emergencyContacts.GetValueOrDefault(emerId);
                ws.Cells[row, col++].Value = emer?.EmergencyContactAddress;
                ws.Cells[row, col++].Value = edu?.ElementarySchool;
                ws.Cells[row, col++].Value = edu?.ElementaryYear;
                ws.Cells[row, col++].Value = edu?.ElementaryHonors;
                ws.Cells[row, col++].Value = edu?.SecondarySchool;
                ws.Cells[row, col++].Value = edu?.SecondaryYear;
                ws.Cells[row, col++].Value = edu?.SecondaryHonors;
                ws.Cells[row, col++].Value = health?.Weight;
                ws.Cells[row, col++].Value = health?.Height;

                row++;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            var bytes = package.GetAsByteArray();
            var stamp = DateTime.Now.ToString("yyyyMMdd");

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"gcams-students-export-{stamp}.xlsx");
        }


        // POST: Students/BulkUpdatePreview
        // Reads the uploaded Excel, matches each row to an existing student by StuID,
        // and shows what would change WITHOUT saving anything yet.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdatePreview(IFormFile file)
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

            var vm = new BulkUpdatePreviewViewModel();
            var matchedRows = new List<StudentBulkRow>();

            // Pull every existing student + related data once, up front, to compare against.
            var existingStudentsList = await _context.Students
                 .Include(s => s.FamilyBackground)
                 .Include(s => s.EmergencyContact)
                 .Include(s => s.EducationalBackground)
                 .Include(s => s.HealthInformation)
                 .ToListAsync();

            // Group instead of ToDictionary, so duplicate StuIDs don't crash the whole preview —
            // take the first match per StuID and flag the rest separately if needed.
            var existingStudents = existingStudentsList
                .GroupBy(s => s.StuID.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= rowCount; row++)
            {
                string? Get(int col)
                {
                    var val = worksheet.Cells[row, col].Text?.Trim();
                    return string.IsNullOrWhiteSpace(val) ? null : val;
                }

                var data = new StudentBulkRow
                {
                    StuID = Get(1) ?? "",
                    StuName = Get(2) ?? "",
                    GradeLevel = Get(3) ?? "",
                    Section = Get(4) ?? "",
                    School = Get(5),
                    Birthday = DateTime.TryParse(Get(6), out var bday) ? bday : null,
                    AcademicYear = Get(7),
                    BirthOrder = Get(8),
                    Address = Get(9) ?? "",
                    StudentContact = Get(10),
                    Email = Get(11),
                    Gender = Get(12),
                    Nationality = Get(13),
                    Religion = Get(14),
                    StayingWith = Get(15),
                    FatherName = Get(16),
                    FatherAge = int.TryParse(Get(17), out var fa) ? fa : null,
                    FatherEducationalAttainment = Get(18),
                    FatherOccupation = Get(19),
                    FatherContact = Get(20),
                    MotherName = Get(21),
                    MotherAge = int.TryParse(Get(22), out var ma) ? ma : null,
                    MotherEducationalAttainment = Get(23),
                    MotherOccupation = Get(24),
                    MotherContact = Get(25),
                    MonthlyFamilyIncome = Get(26),
                    ParentsRelationshipStatus = Get(27),
                    EmergencyContactPerson = Get(28),
                    EmergencyContactAge = int.TryParse(Get(29), out var ea) ? ea : null,
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
                    Height = Get(40)
                };

                if (string.IsNullOrWhiteSpace(data.StuID)) continue; // skip blank rows

                var previewRow = new BulkUpdatePreviewRow { StuID = data.StuID, StuName = data.StuName };

                if (existingStudents.TryGetValue(data.StuID.Trim(), out var existing))
                {
                    previewRow.Found = true;
                    previewRow.ChangedFields = ComputeChangedFields(existing, data);
                    matchedRows.Add(data);
                }
                else
                {
                    previewRow.Found = false; // will be skipped at Confirm — fix StuID typos via Edit instead
                }

                vm.Rows.Add(previewRow);
            }

            vm.PayloadJson = JsonSerializer.Serialize(matchedRows);
            return View(vm);
        }

        // POST: Students/BulkUpdateConfirm
        // Applies the changes the person approved on the preview screen.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateConfirm(string payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                TempData["Error"] = "Nothing to update — the preview data was empty.";
                return RedirectToAction(nameof(Index));
            }

            List<StudentBulkRow>? rows;
            try
            {
                rows = JsonSerializer.Deserialize<List<StudentBulkRow>>(payloadJson);
            }
            catch
            {
                TempData["Error"] = "Could not read the update data. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            if (rows == null || !rows.Any())
            {
                TempData["Error"] = "No rows to update.";
                return RedirectToAction(nameof(Index));
            }

            var stuIds = rows.Select(r => r.StuID.Trim()).ToList();

            var students = await _context.Students
                .Include(s => s.FamilyBackground)
                .Include(s => s.EmergencyContact)
                .Include(s => s.EducationalBackground)
                .Include(s => s.HealthInformation)
                .Where(s => stuIds.Contains(s.StuID))
                .ToListAsync();

            int updatedCount = 0;
            int skippedCount = 0;

            foreach (var data in rows)
            {
                var student = students.FirstOrDefault(s =>
                    string.Equals(s.StuID.Trim(), data.StuID.Trim(), StringComparison.OrdinalIgnoreCase));

                if (student == null) { skippedCount++; continue; } // shouldn't happen, matched at preview time

                student.StuName = data.StuName;
                student.GradeLevel = data.GradeLevel;
                student.Section = data.Section;
                student.School = string.IsNullOrWhiteSpace(data.School)
                    ? student.School
                    : data.School; student.Birthday = data.Birthday;
                student.AcademicYear = data.AcademicYear;
                student.BirthOrder = data.BirthOrder;
                student.Address = data.Address;
                student.Email = data.Email;
                student.Gender = data.Gender;
                student.Nationality = data.Nationality;
                student.Religion = data.Religion;
                student.StayingWith = data.StayingWith;

                // Server-side validation still applies — skip rows that would violate
                // FullName / StudentBirthdate / Required rules rather than saving bad data.
                ModelState.Clear();
                if (!TryValidateModel(student, nameof(Students)))
                {
                    skippedCount++;
                    continue;
                }

                if (student.FamilyBackground != null)
                {
                    student.FamilyBackground.FatherName = data.FatherName;
                    student.FamilyBackground.FatherAge = data.FatherAge;
                    student.FamilyBackground.FatherEducationalAttainment = data.FatherEducationalAttainment;
                    student.FamilyBackground.FatherOccupation = data.FatherOccupation;
                    student.FamilyBackground.MotherName = data.MotherName;
                    student.FamilyBackground.MotherAge = data.MotherAge;
                    student.FamilyBackground.MotherEducationalAttainment = data.MotherEducationalAttainment;
                    student.FamilyBackground.MotherOccupation = data.MotherOccupation;
                    student.FamilyBackground.MonthlyFamilyIncome = data.MonthlyFamilyIncome;
                    student.FamilyBackground.ParentsRelationshipStatus = data.ParentsRelationshipStatus;
                }

                if (student.EmergencyContact != null)
                {
                    student.EmergencyContact.EmergencyContactPerson = data.EmergencyContactPerson;
                    student.EmergencyContact.EmergencyContactAge = data.EmergencyContactAge;
                    student.EmergencyContact.EmergencyContactOccupation = data.EmergencyContactOccupation;
                    student.EmergencyContact.EmergencyContactAddress = data.EmergencyContactAddress;
                }

                if (student.EducationalBackground != null)
                {
                    student.EducationalBackground.ElementarySchool = data.ElementarySchool;
                    student.EducationalBackground.ElementaryYear = data.ElementaryYear;
                    student.EducationalBackground.ElementaryHonors = data.ElementaryHonors;
                    student.EducationalBackground.SecondarySchool = data.SecondarySchool;
                    student.EducationalBackground.SecondaryYear = data.SecondaryYear;
                    student.EducationalBackground.SecondaryHonors = data.SecondaryHonors;
                }

                if (student.HealthInformation != null)
                {
                    student.HealthInformation.Weight = data.Weight;
                    student.HealthInformation.Height = data.Height;
                }

                // Contact numbers: full replace-all per role, same pattern as the Edit page.
                // A blank cell clears that contact — matches what a blank field does on Edit.
                var oldStudentContacts = _context.StudentContactNumbers.Where(x => x.StudentsID == student.StudentsID);
                _context.StudentContactNumbers.RemoveRange(oldStudentContacts);
                if (!string.IsNullOrWhiteSpace(data.StudentContact))
                    _context.StudentContactNumbers.Add(new StudentContactNumber
                    {
                        StudentsID = student.StudentsID,
                        Number = StudentRules.NormalizeMobile(data.StudentContact)
                    });

                if (student.FamilyBackground != null)
                {
                    var famId = student.FamilyBackground.FamilyBackgroundID;
                    var oldFamilyContacts = _context.FamilyContactNumbers.Where(x => x.FamilyBackgroundID == famId);
                    _context.FamilyContactNumbers.RemoveRange(oldFamilyContacts);

                    if (!string.IsNullOrWhiteSpace(data.FatherContact))
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = famId,
                            Number = StudentRules.NormalizeMobile(data.FatherContact),
                            Owner = "Father"
                        });

                    if (!string.IsNullOrWhiteSpace(data.MotherContact))
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = famId,
                            Number = StudentRules.NormalizeMobile(data.MotherContact),
                            Owner = "Mother"
                        });
                }

                if (student.EmergencyContact != null)
                {
                    var emerId = student.EmergencyContact.EmergencyContactID;
                    var oldEmergencyContacts = _context.EmergencyContactNumbers.Where(x => x.EmergencyContactID == emerId);
                    _context.EmergencyContactNumbers.RemoveRange(oldEmergencyContacts);

                    if (!string.IsNullOrWhiteSpace(data.EmergencyContactNumber))
                        _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                        {
                            EmergencyContactID = emerId,
                            Number = StudentRules.NormalizeMobile(data.EmergencyContactNumber)
                        });
                }

                _context.Students.Update(student);
                updatedCount++;
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = User.Identity?.Name ?? "Unknown",
                Date = DateTime.Now,
                ActivityAction = ActivityAction.StudentUpdated.ToString(),
                Details = $"Bulk updated {updatedCount} student(s) via Excel." +
                          (skippedCount > 0 ? $" {skippedCount} row(s) skipped (not found or invalid)." : "")
            });

            // Single commit for every change above, including the log entry.
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{updatedCount} student(s) updated." +
                (skippedCount > 0 ? $" {skippedCount} row(s) skipped." : "");
            return RedirectToAction(nameof(Index));
        }

        // Compares an existing student's data against a parsed Excel row and lists
        // which top-level fields differ, for the preview screen.
        private List<string> ComputeChangedFields(Students existing, StudentBulkRow data)
        {
            var changes = new List<string>();
            void Check(string label, string? oldVal, string? newVal)
            {
                oldVal ??= "";
                newVal ??= "";
                if (!string.Equals(oldVal.Trim(), newVal.Trim(), StringComparison.OrdinalIgnoreCase))
                    changes.Add(label);
            }

            Check("Name", existing.StuName, data.StuName);
            Check("Grade Level", existing.GradeLevel, data.GradeLevel);
            Check("Section", existing.Section, data.Section);
            Check("Address", existing.Address, data.Address);
            Check("Email", existing.Email, data.Email);
            Check("Gender", existing.Gender, data.Gender);
            if (existing.Birthday?.Date != data.Birthday?.Date) changes.Add("Birthday");
            Check("Father's Name", existing.FamilyBackground?.FatherName, data.FatherName);
            Check("Mother's Name", existing.FamilyBackground?.MotherName, data.MotherName);
            Check("Elementary School", existing.EducationalBackground?.ElementarySchool, data.ElementarySchool);

            return changes;
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