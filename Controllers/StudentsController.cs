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
    // [ResponseCache(NoStore = true, ...)] tells the browser NOT to cache any pages
    // from this controller. This is useful for pages with sensitive/changing data
    // (like student records) so users always see the latest info, not a stale cached copy.
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class StudentsController : Controller
    {
        // _context is our connection to the database (via Entity Framework Core).
        // Every table we query or save to (Students, FamilyBackgrounds, etc.) goes through this.
        private readonly AppDbContext _context;

        // Constructor: ASP.NET Core automatically "injects" (provides) the AppDbContext
        // when this controller is created. This is called Dependency Injection.
        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // ===================================================================
        // GET: Students
        // This runs when someone visits "/Students". It shows the list of all students.
        // ===================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch every row from the Students table and turn it into a List.
                // "await" means we pause here (without blocking the server) until the
                // database responds.
                var students = await _context.Students.ToListAsync();

                // Send that list of students to the Index view (the HTML page) to display.
                return View(students);
            }
            catch (Exception ex)
            {
                // If something goes wrong (e.g. database connection issue),
                // just show the raw error text instead of crashing.
                // NOTE: In a real production app you'd normally log this and show
                // a friendlier error page instead of the full exception details.
                return Content(ex.ToString());
            }
        }

        // ===================================================================
        // GET: Students/Details/5
        // Shows the full profile of ONE student (the "5" in the URL is their ID).
        // ===================================================================
        public async Task<IActionResult> Details(int? id)
        {
            // If no ID was given in the URL, there's nothing to look up.
            if (id == null) return NotFound();

            // Fetch the student AND their related records in one query.
            // .Include(...) tells EF Core to also load these connected tables
            // (like a SQL JOIN), otherwise those properties would stay empty/null.
            var student = await _context.Students
                .Include(s => s.FamilyBackground)
                .Include(s => s.EmergencyContact)
                .Include(s => s.EducationalBackground)
                .Include(s => s.HealthInformation)
                .FirstOrDefaultAsync(m => m.StudentsID == id);

            // If no student was found with that ID, show a 404 page.
            if (student == null) return NotFound();

            // Family/Emergency records might not exist yet for this student,
            // so we safely fall back to 0 using "?." and "??" to avoid null errors.
            var familyId = student.FamilyBackground?.FamilyBackgroundID ?? 0;
            var emergencyId = student.EmergencyContact?.EmergencyContactID ?? 0;

            // Build a "ViewModel" — a single object that bundles together everything
            // the Details page needs to display (student info + related records +
            // lists of phone numbers + case notes), so the view only needs one model.
            var vm = new StudentFormViewModel
            {
                Student = student,

                // If a related record doesn't exist yet, use a blank/new one instead
                // of null, so the view doesn't crash trying to read its properties.
                Family = student.FamilyBackground ?? new FamilyBackground(),
                Emergency = student.EmergencyContact ?? new EmergencyContact(),
                Education = student.EducationalBackground ?? new EducationalBackground(),
                Health = student.HealthInformation ?? new HealthInformation(),

                // Get all phone numbers linked to this student directly (e.g. student's own mobile).
                StudentContacts = await _context.StudentContactNumbers
                    .Where(x => x.StudentsID == id)
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                // Get phone numbers belonging to the family record, but only ones
                // labeled "Father" — this table stores both parents' numbers together,
                // so we filter by label to separate them.
                FatherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Label == "Father")
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                // Same idea, but for numbers labeled "Mother".
                MotherContacts = await _context.FamilyContactNumbers
                    .Where(x => x.FamilyBackgroundID == familyId && x.Label == "Mother")
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                // Get all emergency contact numbers linked to this student's emergency record.
                EmergencyContacts = await _context.EmergencyContactNumbers
                    .Where(x => x.EmergencyContactID == emergencyId)
                    .Select(x => new ContactEntry { Number = x.Number, Label = x.Label })
                    .ToListAsync(),

                // Get this student's counseling/case notes, newest first.
                // NOTE: this matches notes by comparing StuName text (not by StudentsID),
                // so it depends on the name being spelled identically in both tables.
                CaseNotes = await _context.CaseNotes
                        .Where(n => n.StudentsID == student.StudentsID)
                        .OrderByDescending(n => n.SessionDate)
                        .ToListAsync(),

                AnecRecs = await _context.AnecRecs
                        .Where(n => n.StudentsID == student.StudentsID)
                        .OrderByDescending(n => n.AnecRecNo)
                        .ToListAsync()

            };

            // Send the fully-built ViewModel to the Details page.
            return View(vm);
        }

        // ===================================================================
        // GET: Students/Create
        // Shows a blank form for adding a new student.
        // ===================================================================
        public IActionResult Create()
        {
            // Pass an empty ViewModel so the form starts with blank fields.
            return View(new StudentFormViewModel());
        }

        // ===================================================================
        // POST: Students/Create
        // This runs when the user submits the "Create" form.
        // ===================================================================
        [HttpPost]
        // Protects against CSRF (Cross-Site Request Forgery) attacks — makes sure
        // the form submission actually came from our own site, not a malicious one.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentFormViewModel vm)
        {
            // ModelState.IsValid checks that all the required fields/validation
            // rules (defined on the models) were satisfied by what the user typed.
            if (ModelState.IsValid)
            {
                // STEP 1: Save the student record first.
                // We need to do this first because the related tables (Family,
                // Emergency, etc.) require the student's ID (a foreign key),
                // which doesn't exist until the student is saved to the database.
                vm.Student.IsActive = true;
                _context.Students.Add(vm.Student);
                await _context.SaveChangesAsync(); // After this, vm.Student.StudentsID is filled in by the DB.

                await EnsureStudentAccountAsync(vm.Student.StuID); // Create a user account for the student if it doesn't exist.

                // STEP 2: Now link the related records to that new student
                // by setting their StudentsID foreign key, then queue them up to be saved.
                vm.Family.StudentsID = vm.Student.StudentsID;
                vm.Emergency.StudentsID = vm.Student.StudentsID;
                vm.Education.StudentsID = vm.Student.StudentsID;
                vm.Health.StudentsID = vm.Student.StudentsID;

                _context.FamilyBackgrounds.Add(vm.Family);
                _context.EmergencyContacts.Add(vm.Emergency);
                _context.EducationalBackgrounds.Add(vm.Education);
                _context.HealthInformations.Add(vm.Health);

                // Save again so the DB generates IDs for Family/Emergency records
                // (we need those IDs in the next step for their phone numbers).
                await _context.SaveChangesAsync(); // get FamilyBackgroundID, EmergencyContactID

                // STEP 3: Save the student's own contact numbers.
                // We loop through every entry the user typed and skip any blank ones.
                foreach (var c in vm.StudentContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.StudentContactNumbers.Add(new StudentContactNumber
                    {
                        StudentsID = vm.Student.StudentsID,  // fixed: was "student.StudentsID"
                        Number = c.Number,
                        Label = c.Label
                    });

                // STEP 4: Save the father's contact numbers, tagging each with "Father"
                // if no specific label was provided.
                foreach (var c in vm.FatherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.FamilyContactNumbers.Add(new FamilyContactNumber
                    {
                        FamilyBackgroundID = vm.Family.FamilyBackgroundID,  // fixed: was "family.FamilyBackgroundID"
                        Number = c.Number,
                        Label = c.Label ?? "Father"
                    });

                // STEP 5: Save the mother's contact numbers the same way.
                foreach (var c in vm.MotherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.FamilyContactNumbers.Add(new FamilyContactNumber
                    {
                        FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                        Number = c.Number,
                        Label = c.Label ?? "Mother"
                    });

                // STEP 6: Save the emergency contact's numbers.
                foreach (var c in vm.EmergencyContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                    _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                    {
                        EmergencyContactID = vm.Emergency.EmergencyContactID,  // fixed: was "emergency.EmergencyContactID"
                        Number = c.Number,
                        Label = c.Label
                    });

                // Save all the contact numbers we just queued up above in one go.
                await _context.SaveChangesAsync();

                // Redirect back to the student list page (this also prevents the
                // browser from re-submitting the form if the user hits refresh).
                return RedirectToAction(nameof(Index));
            }

            // If validation failed, re-show the form with the data the user
            // already typed, plus validation error messages.
            return View(vm);
        }

        // ===================================================================
        // GET: Students/Edit/5
        // Shows the edit form for an existing student, pre-filled with their data.
        // ===================================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Load the student plus all related records (same idea as Details above).
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

                // If a related record doesn't exist yet (e.g. this student never had
                // a Family record created), start a new blank one already linked to
                // this student's ID, so saving it later creates a fresh row correctly.
                Family = student.FamilyBackground ?? new FamilyBackground { StudentsID = student.StudentsID },
                Emergency = student.EmergencyContact ?? new EmergencyContact { StudentsID = student.StudentsID },
                Education = student.EducationalBackground ?? new EducationalBackground { StudentsID = student.StudentsID },
                Health = student.HealthInformation ?? new HealthInformation { StudentsID = student.StudentsID },

                // Load existing contact numbers into the VM lists so the edit form
                // shows what's already saved.
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

        // ===================================================================
        // POST: Students/Edit/5
        // This runs when the user submits changes on the Edit form.
        // ===================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentFormViewModel vm)
        {
            // Safety check: the ID in the URL must match the ID of the student
            // being submitted, otherwise someone could tamper with the form and
            // edit a different student than intended.
            if (id != vm.Student.StudentsID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Mark the student record as "modified" so EF Core will UPDATE it.
                    _context.Update(vm.Student);

                    // For each related record: if its ID is still 0, it means this
                    // student never had one before, so we ADD (insert) a new row.
                    // Otherwise, it already exists, so we UPDATE the existing row.
                    if (vm.Family.FamilyBackgroundID == 0) { vm.Family.StudentsID = id; _context.FamilyBackgrounds.Add(vm.Family); }
                    else _context.FamilyBackgrounds.Update(vm.Family);

                    if (vm.Emergency.EmergencyContactID == 0) { vm.Emergency.StudentsID = id; _context.EmergencyContacts.Add(vm.Emergency); }
                    else _context.EmergencyContacts.Update(vm.Emergency);

                    if (vm.Education.EducationalBackgroundID == 0) { vm.Education.StudentsID = id; _context.EducationalBackgrounds.Add(vm.Education); }
                    else _context.EducationalBackgrounds.Update(vm.Education);

                    if (vm.Health.HealthInformationID == 0) { vm.Health.StudentsID = id; _context.HealthInformations.Add(vm.Health); }
                    else _context.HealthInformations.Update(vm.Health);

                    // Save now so that any newly-created Family/Emergency records
                    // get real IDs from the database before we touch their phone numbers.
                    await _context.SaveChangesAsync(); // ensure FamilyBackgroundID and EmergencyContactID are set

                    // --- Replace contact numbers (delete old, insert new) ---
                    // Simplest way to handle "the user may have added/removed/edited
                    // phone numbers" is to wipe out all the old ones and re-insert
                    // fresh ones from the form, rather than trying to match them up
                    // one by one.

                    // Delete all existing student contact numbers for this student.
                    var oldStudentContacts = _context.StudentContactNumbers.Where(x => x.StudentsID == id);
                    _context.StudentContactNumbers.RemoveRange(oldStudentContacts);

                    // Delete all existing family (father/mother) contact numbers.
                    var oldFamilyContacts = _context.FamilyContactNumbers
                        .Where(x => x.FamilyBackgroundID == vm.Family.FamilyBackgroundID);
                    _context.FamilyContactNumbers.RemoveRange(oldFamilyContacts);

                    // Delete all existing emergency contact numbers.
                    var oldEmergencyContacts = _context.EmergencyContactNumbers
                        .Where(x => x.EmergencyContactID == vm.Emergency.EmergencyContactID);
                    _context.EmergencyContactNumbers.RemoveRange(oldEmergencyContacts);

                    // Commit the deletions before adding new numbers.
                    await _context.SaveChangesAsync(); // clear old ones first

                    // Re-insert the student's contact numbers as currently typed in the form.
                    foreach (var c in vm.StudentContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.StudentContactNumbers.Add(new StudentContactNumber
                        {
                            StudentsID = id,
                            Number = c.Number,
                            Label = c.Label
                        });

                    // Re-insert father's contact numbers.
                    foreach (var c in vm.FatherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                            Number = c.Number,
                            Label = c.Label ?? "Father"
                        });

                    // Re-insert mother's contact numbers.
                    foreach (var c in vm.MotherContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = vm.Family.FamilyBackgroundID,
                            Number = c.Number,
                            Label = c.Label ?? "Mother"
                        });

                    // Re-insert emergency contact numbers.
                    foreach (var c in vm.EmergencyContacts.Where(x => !string.IsNullOrWhiteSpace(x.Number)))
                        _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                        {
                            EmergencyContactID = vm.Emergency.EmergencyContactID,
                            Number = c.Number,
                            Label = c.Label
                        });

                    // Save all the newly inserted contact numbers.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // This happens if two people tried to edit/save the same student
                    // at the same time and the data changed underneath us.
                    // If the student no longer exists at all, show 404.
                    if (!StudentsExists(vm.Student.StudentsID)) return NotFound();
                    // Otherwise, it's a real conflict we don't know how to resolve
                    // automatically, so let the error bubble up.
                    else throw;
                }

                // Success — go back to the list page.
                return RedirectToAction(nameof(Index));
            }

            // Validation failed — show the form again with error messages.
            return View(vm);
        }

        // ===================================================================
        // GET: Students/Delete/5
        // Shows a confirmation page before permanently deleting a student.
        // ===================================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.StudentsID == id);

            if (student == null) return NotFound();

            // Just show the student's info so the user can confirm "yes, delete this one".
            return View(student);
        }

        // ===================================================================
        // POST: Students/Delete/5
        // Runs when the user confirms the delete on the confirmation page.
        // [ActionName("Delete")] lets this method handle POST requests to
        // "/Students/Delete/5" even though the C# method is named "DeleteConfirmed"
        // (this avoids having two C# methods with the exact same name/signature).
        // ===================================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // FindAsync looks the student up by primary key (fast lookup).
            var student = await _context.Students.FindAsync(id);
            if (student != null)
                _context.Students.Remove(student);

            // Note: this is a HARD delete — the row is permanently removed
            // from the database. (Compare with SoftDelete below, which just
            // hides the record instead of erasing it.)
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Small helper used to check whether a student with this ID still
        // exists in the database (used above to handle concurrency conflicts).
        private bool StudentsExists(int id)
        {
            return _context.Students.Any(e => e.StudentsID == id);
        }

        // ===================================================================
        // Soft Delete
        // Instead of removing the student from the database, this just flips
        // their "IsActive" flag to false, so they're hidden from normal views
        // but the data is still kept (e.g. for records/history purposes).
        // ===================================================================
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

        // ===================================================================
        // Restore/Activate
        // The opposite of SoftDelete — brings a previously hidden/inactive
        // student record back to "active" status.
        // ===================================================================
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

        // ===================================================================
        // Import
        // Lets an admin upload an Excel (.xlsx) file containing many students
        // at once, and creates a database record for each row in the sheet.
        // ===================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                // Make sure a file was actually uploaded and it isn't empty.
                if (file == null || file.Length == 0)
                {
                    TempData["Error"] = "Please select an Excel file.";
                    return RedirectToAction(nameof(Index));
                }

                // Only allow .xlsx files — reject anything else (like .csv or .txt)
                // to avoid trying to parse a file format we don't support.
                if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Invalid file format. Please upload an .xlsx file.";
                    return RedirectToAction(nameof(Index));
                }

                // Copy the uploaded file into memory so the Excel library (EPPlus)
                // can read it. "stream.Position = 0" rewinds it back to the start
                // after copying, so reading starts from the beginning.
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                // Open the uploaded bytes as an Excel workbook using EPPlus (OfficeOpenXml).
                using var package = new OfficeOpenXml.ExcelPackage(stream);
                if (package.Workbook.Worksheets.Count == 0)
                {
                    TempData["Error"] = "The Excel file has no worksheets.";
                    return RedirectToAction(nameof(Index));
                }

                // We only look at the first sheet in the workbook.
                var worksheet = package.Workbook.Worksheets[0];

                // How many rows of data are there? (Dimension is null if the sheet is empty.)
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int successCount = 0;

                // Start at row 2 because row 1 is assumed to be the column headers.
                for (int row = 2; row <= rowCount; row++)
                {
                    // Local helper function: reads a cell in the current row/column,
                    // trims whitespace, and returns null instead of an empty string
                    // if the cell is blank. This keeps the code below much shorter,
                    // since we call Get(columnNumber) instead of repeating this logic
                    // for every single field.
                    string? Get(int col)
                    {
                        var val = worksheet.Cells[row, col].Text?.Trim();
                        return string.IsNullOrWhiteSpace(val) ? null : val;
                    }

                    // Build a new Students object by reading each expected column
                    // number from the current row. The column numbers (1, 2, 3, ...)
                    // must match the exact layout of the Excel template being used.
                    var student = new Students
                    {
                        StuID = Get(1) ?? "",
                        StuName = Get(2) ?? "",
                        GradeLevel = Get(3) ?? "",
                        Section = Get(4) ?? "",
                        // If the "School" column is blank, default to this school's full name.
                        School = Get(5) ?? "Don Sergio Osmeña Senior Memorial National High School",
                        // Try to parse the birthday text into a real DateTime; if it
                        // fails (bad/blank text), leave it as null instead of crashing.
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

                    // Skip this row entirely if the essential fields (name or ID)
                    // are missing — treat it as a blank/invalid row rather than
                    // saving an incomplete student.
                    if (string.IsNullOrWhiteSpace(student.StuName) || string.IsNullOrWhiteSpace(student.StuID))
                        continue;

                    // Save the student first so we get a real StudentsID,
                    // which the related records below need as a foreign key.
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    await EnsureStudentAccountAsync(student.StuID); // Create a user account for the student if it doesn't exist.

                    // Column 10 = student's own contact number (if provided).
                    if (Get(10) is string stuContact)
                        _context.StudentContactNumbers.Add(new StudentContactNumber
                        {
                            StudentsID = student.StudentsID,
                            Number = stuContact,
                            Label = "Mobile"
                        });

                    // Build the family background record from columns 16-27.
                    var family = new FamilyBackground
                    {
                        StudentsID = student.StudentsID,
                        FatherName = Get(16),
                        // Try to parse age as a whole number; if it fails, leave it null.
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
                    // Save now so "family.FamilyBackgroundID" gets a real value
                    // before we use it below for the parents' phone numbers.
                    await _context.SaveChangesAsync(); // get FamilyBackgroundID

                    // Column 20 = father's contact number.
                    if (Get(20) is string fatherContact)
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = family.FamilyBackgroundID,
                            Number = fatherContact,
                            Label = "Father"
                        });

                    // Column 25 = mother's contact number.
                    if (Get(25) is string motherContact)
                        _context.FamilyContactNumbers.Add(new FamilyContactNumber
                        {
                            FamilyBackgroundID = family.FamilyBackgroundID,
                            Number = motherContact,
                            Label = "Mother"
                        });

                    // Build the emergency contact record from columns 28-32.
                    var emergency = new EmergencyContact
                    {
                        StudentsID = student.StudentsID,
                        EmergencyContactPerson = Get(28),
                        EmergencyContactAge = int.TryParse(Get(29), out int ecAge) ? ecAge : null,
                        EmergencyContactOccupation = Get(30),
                        EmergencyContactAddress = Get(32),
                    };
                    _context.EmergencyContacts.Add(emergency);
                    // Save now so we get a real EmergencyContactID for the phone number below.
                    await _context.SaveChangesAsync(); // get EmergencyContactID

                    // Column 31 = emergency contact's phone number.
                    if (Get(31) is string emergencyContact)
                        _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                        {
                            EmergencyContactID = emergency.EmergencyContactID,
                            Number = emergencyContact,
                            Label = "Mobile"
                        });

                    // Build the educational background record from columns 33-38.
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

                    // Build the health information record from columns 39-40.
                    _context.HealthInformations.Add(new HealthInformation
                    {
                        StudentsID = student.StudentsID,
                        Weight = Get(39),
                        Height = Get(40),
                    });

                    // Save everything queued for this row (education + health records).
                    await _context.SaveChangesAsync();
                    successCount++;
                }

                // Let the user know how many students were successfully imported.
                TempData["Success"] = $"{successCount} student(s) imported successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // If anything unexpected goes wrong during import (bad file,
                // unexpected format, DB error, etc.), log it to the console
                // and show a friendly error message to the user instead of crashing.
                Console.WriteLine(ex.ToString());
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        //-----------------------------------
        // This is for the accounts
        //-----------------------------------

            public static bool VerifyHash(string password, byte[] salt, byte[] hash)
            {
                // Hash the provided password with the same salt
                var hashedPassword = UsersController.HashPassword(password, salt);

                // Compare the hashed password with the stored hash
                return hashedPassword.SequenceEqual(hash);
            }



            //For student account
            private async Task EnsureStudentAccountAsync(string stuID)
            {
                if (string.IsNullOrWhiteSpace(stuID)) return;

                bool exists = await _context.Users.AnyAsync(u => u.Username == stuID);
                if (exists) return;

                //Generate Salt

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





















    }
}