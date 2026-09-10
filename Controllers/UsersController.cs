using GCAMS.Data;
using GCAMS.Models.ActivityLogs;
using GCAMS.Models.Counselor;
using GCAMS.Models.Students;
using GCAMS.ViewModels;
using GCAMS.Models.Users;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace GCAMS.Controllers
{
    public class UsersController : Controller
    {

        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }


        [Route("Login")]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Someone already signed in has no business on the login screen.
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // One message for "no such user" and "wrong password" alike, so the page
            // cannot be used to work out which usernames exist.
            const string genericError = "Invalid username or password.";

            username = (username ?? string.Empty).Trim();

            if (username.Length == 0 || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = genericError;
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            bool passwordOk = false;
            if (user != null)
            {
                try
                {
                    byte[] hash = HashPassword(password, Convert.FromBase64String(user.Salt));
                    passwordOk = CryptographicOperations.FixedTimeEquals(
                        hash, Convert.FromBase64String(user.Password));
                }
                catch (FormatException)
                {
                    // Salt or hash on this row is not valid base64 — treat as a failed
                    // login rather than throwing a 500 at the user.
                    passwordOk = false;
                }
            }

            if (user == null || !passwordOk)
            {
                await LogSignInFailureAsync(username,
                    user == null ? "no such username" : "incorrect password");
                ViewBag.ErrorMessage = genericError;
                return View();
            }

            // Checked AFTER the password, so a deactivated account cannot be confirmed
            // by anyone who does not already know its password.
            if (!user.IsActive)
            {
                await LogSignInFailureAsync(username, "account is deactivated");
                ViewBag.ErrorMessage = "This account has been deactivated. Please contact an administrator.";
                return View();
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = user.Username,
                Date = DateTime.Now,
                ActivityAction = ActivityAction.SignIn.ToString(),
                Details = $"{user.Username} signed in."
            });
            await _context.SaveChangesAsync();

            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
                    new Claim("PasswordChange", user.PasswordChange.ToString())
                },
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            // PasswordChange == false means "must still change it" (the Program.cs
            // middleware reads it the same way). Send them straight there.
            if (!user.PasswordChange)
                return RedirectToAction(nameof(ChangePass));

            // IsLocalUrl blocks an open redirect: without it, /Login?ReturnUrl=https://evil.site
            // would bounce a freshly signed-in user off to another domain.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        private async Task LogSignInFailureAsync(string username, string reason)
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = string.IsNullOrWhiteSpace(username) ? "Unknown" : username,
                Date = DateTime.Now,
                ActivityAction = ActivityAction.SignInFailed.ToString(),
                Details = $"Failed sign-in for '{username}' ({reason})."
            });
            await _context.SaveChangesAsync();
        }


        private static readonly Regex PasswordPolicy = new Regex(
    @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$",
    RegexOptions.Compiled);

        [HttpGet("ChangePass")]
        [Authorize]
        public IActionResult ChangePass()
        {
            var mustChange = User.FindFirst("PasswordChange")?.Value
                .Equals("false", StringComparison.OrdinalIgnoreCase) == true;
            ViewBag.Forced = mustChange;
            return View();
        }

        [HttpPost("ChangePass")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePass(string currentPassword, string newPassword, string confirmPassword)
        {
            // Re-derive this so the view still knows whether the change was forced
            // even after we bounce back here with an error.
            ViewBag.Forced = User.FindFirst("PasswordChange")?.Value
                .Equals("false", StringComparison.OrdinalIgnoreCase) == true;

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                ViewBag.ErrorMessage = "Password must be at least 8 characters.";
                return View();
            }

            if (!PasswordPolicy.IsMatch(newPassword))
            {
                ViewBag.ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a number, and a special character (!@#$%^&*).";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "New passwords do not match.";
                return View();
            }

            var username = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login");

            byte[] currentHash = HashPassword(currentPassword, Convert.FromBase64String(user.Salt));
            if (!CryptographicOperations.FixedTimeEquals(currentHash, Convert.FromBase64String(user.Password)))
            {
                ViewBag.ErrorMessage = "Current password is incorrect.";
                return View();
            }

            byte[] newSalt = CreateSalt();
            byte[] newHash = HashPassword(newPassword, newSalt);

            user.Salt = Convert.ToBase64String(newSalt);
            user.Password = Convert.ToBase64String(newHash);
            user.PasswordChange = true;

            await _context.SaveChangesAsync();

            // Activity Log
            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = user.Username,
                Date = DateTime.Now,
                ActivityAction = ActivityAction.PasswordChanged.ToString(),
                Details = $"{user.Username} changed their password."
            });
            await _context.SaveChangesAsync();

            var identity = new ClaimsIdentity(
                new[]
                {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("PasswordChange", "true")
                },
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }



        public static byte[] CreateSalt()
        {
            var buffer = new byte[16];
            RandomNumberGenerator.Fill(buffer);
            return buffer;
        }


        public static byte[] HashPassword(string password, byte[] salt)
        {
            // Use Argon2id for hashing the password
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8; // Number of threads to use
                argon2.MemorySize = 65536; // 64 MB
                argon2.Iterations = 4; // Number of iterations

                return argon2.GetBytes(32);
            }

        }


        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }


        // ===================================================================
        // MY PROFILE
        //
        // Self-service page. Every role can reach it (the sidebar already links
        // to Users/MyProfile), but what is editable differs because the three
        // roles do not share a data model:
        //
        //   Student   -> Students + HealthInformation + EmergencyContact
        //   Counselor -> Counselor
        //   Admin     -> Users only, which holds no personal data at all
        //
        // SECURITY NOTE: the POST never binds an entity. It binds
        // MyProfileViewModel, which has no StuName / StuID / GradeLevel / Role
        // property, then copies the whitelist onto the tracked entity by hand.
        // Adding <input name="StuName"> in dev tools therefore does nothing.
        // ===================================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var vm = await BuildProfileAsync();
            if (vm == null) return RedirectToAction(nameof(Login));
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(MyProfileViewModel posted)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return RedirectToAction(nameof(Login));

            var account = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (account == null) return RedirectToAction(nameof(Login));

            var role = account.Role ?? string.Empty;

            // Rebuild the display half from the database. Anything the browser
            // sent for those fields is ignored outright.
            var vm = await BuildProfileAsync();
            if (vm == null) return RedirectToAction(nameof(Login));

            // Admins have nothing to save; don't pretend otherwise.
            if (!vm.HasEditableFields) return RedirectToAction(nameof(MyProfile));

            // Carry the submitted values onto the rebuilt view model so the form
            // redisplays what the user typed if validation fails.
            if (vm.EmailIsEditable) vm.Email = posted.Email;
            vm.Address = posted.Address;
            vm.StayingWith = posted.StayingWith;
            vm.BloodType = posted.BloodType;
            vm.Height = posted.Height;
            vm.Weight = posted.Weight;
            vm.EmergencyContactPerson = posted.EmergencyContactPerson;
            vm.EmergencyContactAge = posted.EmergencyContactAge;
            vm.EmergencyContactOccupation = posted.EmergencyContactOccupation;
            vm.EmergencyContactAddress = posted.EmergencyContactAddress;
            vm.ContactNumbers = CleanNumbers(posted.ContactNumbers);
            vm.EmergencyContactNumbers = CleanNumbers(posted.EmergencyContactNumbers);

            // ModelState still holds the raw posted keys. Drop the ones that map to
            // display-only text so a stale read-only value can never block a save.
            foreach (var key in ModelState.Keys.ToList())
            {
                if (key.StartsWith("ReadOnlyFacts") ||
                    key.StartsWith("ContactNumbers") ||
                    key.StartsWith("EmergencyContactNumbers") ||
                    key == nameof(MyProfileViewModel.DisplayName) ||
                    key == nameof(MyProfileViewModel.IdentifierValue) ||
                    key == nameof(MyProfileViewModel.Username) ||
                    key == nameof(MyProfileViewModel.Role))
                {
                    ModelState.Remove(key);
                }
            }

            ValidateNumbers(vm.ContactNumbers, nameof(MyProfileViewModel.ContactNumbers));
            ValidateNumbers(vm.EmergencyContactNumbers, nameof(MyProfileViewModel.EmergencyContactNumbers));

            if (!ModelState.IsValid) return View(vm);

            if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                await SaveStudentProfileAsync(username, vm);
            }
            else if (string.Equals(role, "Counselor", StringComparison.OrdinalIgnoreCase))
            {
                await SaveCounselorProfileAsync(username, vm);
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = username,
                Date = DateTime.Now,
                ActivityAction = ActivityAction.AccountUpdated.ToString(),
                Details = $"{username} updated their own profile."
            });
            await _context.SaveChangesAsync();

            TempData["ProfileSaved"] = "Your profile has been updated.";
            return RedirectToAction(nameof(MyProfile));
        }

        // -------------------------------------------------------------------
        // Build the view model for whoever is signed in.
        // -------------------------------------------------------------------
        private async Task<MyProfileViewModel?> BuildProfileAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return null;

            var account = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (account == null) return null;

            var vm = new MyProfileViewModel
            {
                Role = account.Role ?? string.Empty,
                Username = account.Username,
                AccountIsActive = account.IsActive,
                DisplayName = account.Username,
                IdentifierLabel = "Username",
                IdentifierValue = account.Username
            };

            if (string.Equals(vm.Role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                // Student accounts are created with Username == StuID.
                var student = await _context.Students
                    .Include(s => s.HealthInformation)
                    .Include(s => s.EmergencyContact)
                        .ThenInclude(e => e!.ContactNumbers)
                    .FirstOrDefaultAsync(s => s.StuID == username);

                if (student == null)
                {
                    vm.LockedFieldNote = "No student record is linked to this account yet. Please ask the Care Center to check your record.";
                    return vm;
                }

                vm.DisplayName = student.StuName;
                vm.IdentifierLabel = "Student ID";
                vm.IdentifierValue = student.StuID;

                vm.ReadOnlyFacts = new List<ProfileFact>
                {
                    new ProfileFact("Full Name", student.StuName),
                    new ProfileFact("Student ID", student.StuID),
                    new ProfileFact("Grade Level", student.GradeLevel),
                    new ProfileFact("Section", student.Section),
                    new ProfileFact("School", student.School),
                    new ProfileFact("Academic Year", student.AcademicYear),
                    new ProfileFact("Date of Birth", student.Birthday.HasValue ? student.Birthday.Value.ToString("MMMM d, yyyy") : null),
                    new ProfileFact("Age", student.Birthday.HasValue ? student.Age.ToString() : null),
                    new ProfileFact("Gender", student.Gender)
                };

                vm.HasEditableFields = true;
                vm.ShowEmail = true;
                vm.EmailIsEditable = true;
                vm.ShowContactNumbers = true;
                vm.ShowAddress = true;
                vm.ShowLivingArrangement = true;
                vm.ShowHealth = true;
                vm.ShowEmergency = true;

                vm.Email = student.Email;
                vm.Address = student.Address;
                vm.StayingWith = student.StayingWith;

                vm.ContactNumbers = await _context.StudentContactNumbers
                    .Where(c => c.StudentsID == student.StudentsID)
                    .Select(c => c.Number)
                    .ToListAsync();

                vm.BloodType = student.HealthInformation != null ? student.HealthInformation.BloodType : null;
                vm.Height = student.HealthInformation != null ? student.HealthInformation.Height : null;
                vm.Weight = student.HealthInformation != null ? student.HealthInformation.Weight : null;

                if (student.EmergencyContact != null)
                {
                    vm.EmergencyContactPerson = student.EmergencyContact.EmergencyContactPerson;
                    vm.EmergencyContactAge = student.EmergencyContact.EmergencyContactAge;
                    vm.EmergencyContactOccupation = student.EmergencyContact.EmergencyContactOccupation;
                    vm.EmergencyContactAddress = student.EmergencyContact.EmergencyContactAddress;
                    vm.EmergencyContactNumbers = student.EmergencyContact.ContactNumbers
                        .Select(c => c.Number).ToList();
                }
            }
            else if (string.Equals(vm.Role, "Counselor", StringComparison.OrdinalIgnoreCase))
            {
                // Counselor accounts are created with Username == EmailAddress.
                var counselor = await _context.Counselors
                    .Include(c => c.ContactNumbers)
                    .FirstOrDefaultAsync(c => c.EmailAddress == username);

                if (counselor == null)
                {
                    vm.LockedFieldNote = "No counselor record is linked to this account yet. Please ask an administrator to check your record.";
                    return vm;
                }

                vm.DisplayName = counselor.CounselorName;
                vm.IdentifierLabel = "Employee Number";
                vm.IdentifierValue = counselor.EmployeeNumber;

                vm.ReadOnlyFacts = new List<ProfileFact>
                {
                    new ProfileFact("Full Name", counselor.CounselorName),
                    new ProfileFact("Employee Number", counselor.EmployeeNumber),
                    new ProfileFact("Position", counselor.Position),
                    new ProfileFact("Employment Status", counselor.EmploymentStatus),
                    new ProfileFact("Date of Birth", counselor.BirthDate == default(DateTime) ? null : counselor.BirthDate.ToString("MMMM d, yyyy")),
                    new ProfileFact("Gender", counselor.Gender),
                    new ProfileFact("Work/School", counselor.WorkSchool)
                };

                vm.HasEditableFields = true;
                vm.ShowEmail = true;

                // Locked on purpose: this email IS the login username. Renaming it
                // here would break the signed-in cookie and lock the user out.
                vm.EmailIsEditable = false;
                vm.LockedFieldNote = "Your email address doubles as your login username, so only an administrator can change it.";

                vm.ShowContactNumbers = true;
                vm.ShowAddress = true;

                vm.Email = counselor.EmailAddress;
                vm.Address = counselor.Address;
                vm.ContactNumbers = counselor.ContactNumbers.Select(c => c.Number).ToList();
            }
            else
            {
                // Admin. Users holds no personal data, so there is nothing to edit.
                vm.ReadOnlyFacts = new List<ProfileFact>
                {
                    new ProfileFact("Username", account.Username),
                    new ProfileFact("Role", account.Role),
                    new ProfileFact("Account Status", account.IsActive ? "Active" : "Inactive")
                };
                vm.HasEditableFields = false;
            }

            return vm;
        }

        // -------------------------------------------------------------------
        // Save. Only whitelisted properties are ever assigned.
        // -------------------------------------------------------------------
        private async Task SaveStudentProfileAsync(string username, MyProfileViewModel vm)
        {
            var student = await _context.Students
                .Include(s => s.HealthInformation)
                .Include(s => s.EmergencyContact)
                .FirstOrDefaultAsync(s => s.StuID == username);

            if (student == null) return;

            student.Email = vm.Email;
            student.StayingWith = vm.StayingWith;
            if (!string.IsNullOrWhiteSpace(vm.Address)) student.Address = vm.Address;

            // --- health ---
            if (student.HealthInformation == null)
            {
                student.HealthInformation = new HealthInformation { StudentsID = student.StudentsID };
                _context.HealthInformations.Add(student.HealthInformation);
            }
            student.HealthInformation.BloodType = vm.BloodType;
            student.HealthInformation.Height = vm.Height;
            student.HealthInformation.Weight = vm.Weight;

            // --- emergency contact ---
            if (student.EmergencyContact == null)
            {
                student.EmergencyContact = new EmergencyContact { StudentsID = student.StudentsID };
                _context.EmergencyContacts.Add(student.EmergencyContact);
            }
            student.EmergencyContact.EmergencyContactPerson = vm.EmergencyContactPerson;
            student.EmergencyContact.EmergencyContactAge = vm.EmergencyContactAge;
            student.EmergencyContact.EmergencyContactOccupation = vm.EmergencyContactOccupation;
            student.EmergencyContact.EmergencyContactAddress = vm.EmergencyContactAddress;

            // Saved here so a freshly created child row has its identity value
            // before the contact numbers below reference it.
            await _context.SaveChangesAsync();

            // --- own numbers: replace the whole set ---
            var existing = await _context.StudentContactNumbers
                .Where(c => c.StudentsID == student.StudentsID)
                .ToListAsync();
            _context.StudentContactNumbers.RemoveRange(existing);

            foreach (var number in vm.ContactNumbers)
            {
                _context.StudentContactNumbers.Add(new StudentContactNumber
                {
                    StudentsID = student.StudentsID,
                    Number = StudentRules.NormalizeMobile(number)
                });
            }

            // --- emergency numbers: replace the whole set ---
            var existingEmergency = await _context.EmergencyContactNumbers
                .Where(c => c.EmergencyContactID == student.EmergencyContact.EmergencyContactID)
                .ToListAsync();
            _context.EmergencyContactNumbers.RemoveRange(existingEmergency);

            foreach (var number in vm.EmergencyContactNumbers)
            {
                _context.EmergencyContactNumbers.Add(new EmergencyContactNumber
                {
                    EmergencyContactID = student.EmergencyContact.EmergencyContactID,
                    Number = StudentRules.NormalizeMobile(number)
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task SaveCounselorProfileAsync(string username, MyProfileViewModel vm)
        {
            var counselor = await _context.Counselors
                .FirstOrDefaultAsync(c => c.EmailAddress == username);

            if (counselor == null) return;

            // EmailAddress is deliberately NOT assigned here - it is the username.
            if (!string.IsNullOrWhiteSpace(vm.Address)) counselor.Address = vm.Address;

            var existing = await _context.CounselorContactNumbers
                .Where(c => c.CounselorID == counselor.CounselorID)
                .ToListAsync();
            _context.CounselorContactNumbers.RemoveRange(existing);

            foreach (var number in vm.ContactNumbers)
            {
                _context.CounselorContactNumbers.Add(new CounselorContactNumber
                {
                    CounselorID = counselor.CounselorID,
                    Number = StudentRules.NormalizeMobile(number)
                });
            }

            await _context.SaveChangesAsync();
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------
        private static List<string> CleanNumbers(List<string> raw)
        {
            if (raw == null) return new List<string>();
            return raw.Where(n => !string.IsNullOrWhiteSpace(n))
                      .Select(n => n.Trim())
                      .ToList();
        }

        private void ValidateNumbers(List<string> numbers, string key)
        {
            for (int i = 0; i < numbers.Count; i++)
            {
                if (!StudentRules.IsValidMobile(numbers[i]))
                {
                    ModelState.AddModelError(key + "[" + i + "]",
                        "'" + numbers[i] + "' is not a valid Philippine mobile number.");
                }
            }
        }


        // ###################################################################
        // ADMIN ACCOUNT MANAGEMENT
        //
        // Student and counselor accounts are created as a side effect of adding
        // the person (Students/Create, Counselors/Create). Admins have no person
        // record, so this is the only place an admin account can be created.
        //
        // Accounts are NEVER deleted, only deactivated: ActivityLogs.Who stores a
        // username as plain text, so deleting a row would leave history attributed
        // to someone who no longer exists.
        //
        // NOTE: [Authorize(Roles = "Admin")] is applied PER ACTION, not on the
        // class -- Login must stay anonymous, and ChangePass/MyProfile/Logout must
        // stay open to every signed-in role.
        // ###################################################################

        // ===================================================================
        // LIST
        // ===================================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? roleFilter, string? statusFilter)
        {
            var me = User.Identity?.Name ?? string.Empty;

            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u => u.Username.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter))
                query = query.Where(u => u.Role == roleFilter);

            if (statusFilter == "Active")
                query = query.Where(u => u.IsActive);
            else if (statusFilter == "Inactive")
                query = query.Where(u => !u.IsActive);

            var users = await query
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Username)
                .ToListAsync();

            // Resolve display names in two batched lookups rather than per row.
            var usernames = users.Select(u => u.Username).ToList();

            var studentNames = await _context.Students.AsNoTracking()
                .Where(s => usernames.Contains(s.StuID))
                .ToDictionaryAsync(s => s.StuID, s => s.StuName);

            var counselorNames = await _context.Counselors.AsNoTracking()
                .Where(c => usernames.Contains(c.EmailAddress))
                .ToDictionaryAsync(c => c.EmailAddress, c => c.CounselorName);

            var activeAdmins = await _context.Users.CountAsync(u => u.Role == "Admin" && u.IsActive);

            var rows = new List<AccountRowViewModel>();
            foreach (var u in users)
            {
                string? linked = null;
                if (studentNames.TryGetValue(u.Username, out var sn)) linked = sn;
                else if (counselorNames.TryGetValue(u.Username, out var cn)) linked = cn;

                var isSelf = string.Equals(u.Username, me, StringComparison.OrdinalIgnoreCase);

                string? lockReason = null;
                if (isSelf)
                    lockReason = "You cannot deactivate your own account.";
                else if (u.Role == "Admin" && u.IsActive && activeAdmins <= 1)
                    lockReason = "This is the last active administrator.";

                rows.Add(new AccountRowViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    PasswordChange = u.PasswordChange,
                    LinkedName = linked,
                    IsSelf = isSelf,
                    LockReason = lockReason
                });
            }

            var vm = new AccountIndexViewModel
            {
                Accounts = rows,
                Search = search,
                RoleFilter = roleFilter,
                StatusFilter = statusFilter,
                TotalCount = await _context.Users.CountAsync(),
                ActiveCount = await _context.Users.CountAsync(u => u.IsActive),
                AdminCount = activeAdmins
            };

            return View(vm);
        }

        // ===================================================================
        // CREATE ADMIN
        // ===================================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View(new CreateAdminViewModel());

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAdminViewModel vm)
        {
            var username = (vm.Username ?? string.Empty).Trim();
            vm.Username = username;

            if (await _context.Users.AnyAsync(u => u.Username == username))
                ModelState.AddModelError(nameof(vm.Username), "That username is already taken.");

            if (!ModelState.IsValid) return View(vm);

            byte[] salt = CreateSalt();
            byte[] hash = HashPassword(vm.Password, salt);

            _context.Users.Add(new Users
            {
                Username = username,
                Salt = Convert.ToBase64String(salt),
                Password = Convert.ToBase64String(hash),
                Role = "Admin",
                IsActive = true,
                // false == "must still change it", which is how Program.cs reads it.
                PasswordChange = false
            });

            await LogAsync(ActivityAction.AccountCreated,
                $"Created administrator account '{username}'.");

            await _context.SaveChangesAsync();

            TempData["AccountMessage"] = $"Administrator '{username}' created. They will be asked to set a new password at first sign-in.";
            return RedirectToAction(nameof(Index));
        }

        // ===================================================================
        // DEACTIVATE / REACTIVATE
        // ===================================================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(int id, bool active)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            var me = User.Identity?.Name ?? string.Empty;

            // Rail 1: never lock yourself out.
            if (string.Equals(user.Username, me, StringComparison.OrdinalIgnoreCase))
            {
                TempData["AccountError"] = "You cannot change the status of your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Rail 2: never leave the system with zero admins who can sign in.
            if (!active && user.Role == "Admin" && user.IsActive)
            {
                var activeAdmins = await _context.Users.CountAsync(u => u.Role == "Admin" && u.IsActive);
                if (activeAdmins <= 1)
                {
                    TempData["AccountError"] = "This is the last active administrator and cannot be deactivated.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (user.IsActive == active)
                return RedirectToAction(nameof(Index));   // nothing to do

            user.IsActive = active;

            await LogAsync(active ? ActivityAction.AccountUpdated : ActivityAction.AccountRemoved,
                $"{(active ? "Reactivated" : "Deactivated")} account '{user.Username}'.");

            await _context.SaveChangesAsync();

            TempData["AccountMessage"] = active
                ? $"'{user.Username}' can sign in again."
                : $"'{user.Username}' has been deactivated and can no longer sign in.";

            return RedirectToAction(nameof(Index));
        }

        // ===================================================================
        // RESET PASSWORD
        // ===================================================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == vm.UserId);
            if (user == null) return NotFound();

            if (string.Equals(user.Username, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
            {
                TempData["AccountError"] = "Use Change Password to update your own password.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var first = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "That password is not valid.";
                TempData["AccountError"] = first;
                return RedirectToAction(nameof(Index));
            }

            byte[] salt = CreateSalt();
            byte[] hash = HashPassword(vm.NewPassword, salt);

            user.Salt = Convert.ToBase64String(salt);
            user.Password = Convert.ToBase64String(hash);

            // Force a change at next sign-in so the admin-chosen password is temporary.
            user.PasswordChange = false;

            await LogAsync(ActivityAction.PasswordChanged,
                $"Reset the password for '{user.Username}'.");

            await _context.SaveChangesAsync();

            TempData["AccountMessage"] = $"Password reset for '{user.Username}'. They must set a new one at next sign-in.";
            return RedirectToAction(nameof(Index));
        }

        // ===================================================================
        private async Task LogAsync(ActivityAction action, string details)
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = User.Identity?.Name ?? "Unknown",
                Date = DateTime.Now,
                ActivityAction = action.ToString(),
                Details = details
            });
            await Task.CompletedTask;
        }
    }
}