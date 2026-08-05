using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GCAMS.Data;
using GCAMS.Models.Counselor;
using System.Security.Cryptography;
using GCAMS.Models.Users;

namespace GCAMS.Controllers
{
    public class CounselorsController : Controller
    {
        // Our connection to the database (Entity Framework Core).
        private readonly AppDbContext _context;

        public CounselorsController(AppDbContext context)
        {
            _context = context;
        }

        // ===================================================================
        // GET: Counselors
        // Shows the list of all counselors.
        // ===================================================================
        public async Task<IActionResult> Index()
        {
            return View(await _context.Counselors.ToListAsync());
        }

        // ===================================================================
        // GET: Counselors/Details/5
        // Shows the full profile of ONE counselor (5 = their ID from the URL).
        // ===================================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var counselor = await _context.Counselors
                .Include(c => c.ContactNumbers)
                .FirstOrDefaultAsync(m => m.CounselorID == id);

            if (counselor == null) return NotFound();
            return View(counselor);
        }

        // ===================================================================
        // GET: Counselors/Create
        // Shows a blank form for adding a new counselor.
        // ===================================================================
        public IActionResult Create()
        {
            return View();
        }

        // ===================================================================
        // POST: Counselors/Create
        // Runs when the user submits the "Create" form.
        // ===================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Counselor counselor)
        {
            if (ModelState.IsValid)
            {
                var incoming = counselor.ContactNumbers
                    .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                    .ToList();

                counselor.ContactNumbers.Clear();

                _context.Counselors.Add(counselor);
                await _context.SaveChangesAsync();

                await EnsureCounselorAccountAsync(counselor.EmailAddress, counselor.EmployeeNumber);

                foreach (var c in incoming)
                {
                    _context.CounselorContactNumbers.Add(new CounselorContactNumber
                    {
                        CounselorID = counselor.CounselorID,
                        Number = c.Number,
                        Label = c.Label
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(counselor);
        }

        // ===================================================================
        // GET: Counselors/Edit/5
        // Shows the edit form for an existing counselor, pre-filled with their data.
        // ===================================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var counselor = await _context.Counselors
                .Include(c => c.ContactNumbers)
                .FirstOrDefaultAsync(c => c.CounselorID == id);

            if (counselor == null) return NotFound();
            return View(counselor);
        }

        // ===================================================================
        // POST: Counselors/Edit/5
        // Runs when the user submits changes on the Edit form.
        // ===================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Counselor counselor)
        {
            if (id != counselor.CounselorID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Grab what's on file *before* we overwrite it, so we know
                    // whether the linked Users.Username needs to move with it.
                    var oldEmail = await _context.Counselors
                        .Where(c => c.CounselorID == id)
                        .Select(c => c.EmailAddress)
                        .FirstOrDefaultAsync();

                    var incoming = counselor.ContactNumbers
                        .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                        .ToList();

                    counselor.ContactNumbers.Clear();

                    _context.Update(counselor);

                    var old = _context.CounselorContactNumbers.Where(x => x.CounselorID == id);
                    _context.CounselorContactNumbers.RemoveRange(old);
                    await _context.SaveChangesAsync();

                    foreach (var c in incoming)
                    {
                        _context.CounselorContactNumbers.Add(new CounselorContactNumber
                        {
                            CounselorID = id,
                            Number = c.Number,
                            Label = c.Label
                        });
                    }

                    await _context.SaveChangesAsync();

                    // Either move the existing login to the new email, or —
                    // if this counselor predates the account feature — create one now.
                    await SyncCounselorAccountAsync(oldEmail, counselor.EmailAddress, counselor.EmployeeNumber);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Counselors.Any(e => e.CounselorID == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(counselor);
        }

        private async Task SyncCounselorAccountAsync(string oldEmail, string newEmail, string employeeNumber)
        {
            if (string.IsNullOrWhiteSpace(newEmail)) return;

            // No prior email on file, or the account was never created — create it now.
            if (string.IsNullOrWhiteSpace(oldEmail))
            {
                await EnsureCounselorAccountAsync(newEmail, employeeNumber);
                return;
            }

            if (string.Equals(oldEmail, newEmail, StringComparison.OrdinalIgnoreCase)) return;

            var account = await _context.Users.FirstOrDefaultAsync(u => u.Username == oldEmail);
            if (account == null)
            {
                await EnsureCounselorAccountAsync(newEmail, employeeNumber);
                return;
            }

            account.Username = newEmail;
            await _context.SaveChangesAsync();
        }

        private async Task EnsureCounselorAccountAsync(string email, string employeeNumber)
        {
            if (string.IsNullOrWhiteSpace(email)) return;

            bool exists = await _context.Users.AnyAsync(u => u.Username == email);
            if (exists) return;

            byte[] salt = UsersController.CreateSalt();
            byte[] hash = UsersController.HashPassword(employeeNumber, salt);

            var account = new Users
            {
                Username = email,
                Password = Convert.ToBase64String(hash),
                Salt = Convert.ToBase64String(salt),
                Role = "Counselor",
                PasswordChange = false, // forces the change-password flow on first login
            };

            _context.Users.Add(account);
            await _context.SaveChangesAsync();
        }


        // GET: Counselors/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null) return NotFound();

        //    var counselor = await _context.Counselors
        //        .FirstOrDefaultAsync(m => m.CounselorID == id);

        //    if (counselor == null) return NotFound();
        //    return View(counselor);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SoftDelete(int id)
        //{
        //    var counselor = await _context.Counselors.FindAsync(id);
        //    if (counselor == null) return NotFound();

        //    counselor.EmploymentStatus = "Permanent";
        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Counselor has been set to inactive.";
        //    return RedirectToAction(nameof(Index));
        //}

        //// POST: Counselors/Restore/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Restore(int id)
        //{
        //    var counselor = await _context.Counselors.FindAsync(id);
        //    if (counselor == null) return NotFound();

        //    counselor.EmploymentStatus = "Permanent";
        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Counselor has been set to active.";
        //    return RedirectToAction(nameof(Index));
        //}

        // Small helper that checks whether a counselor with this ID still
        // exists in the database. (Currently unused now that Delete is
        // commented out, but kept around in case it's needed again.)
        private bool CounselorExists(int id)
        {
            return _context.Counselors.Any(e => e.CounselorID == id);
        }
    }
}