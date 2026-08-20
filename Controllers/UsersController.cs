using GCAMS.Data;
using GCAMS.Models.ActivityLogs;
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
        public IActionResult Login()
        {

            return View();
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user != null)
            {
                byte[] hash = HashPassword(password, Convert.FromBase64String(user.Salt));
                if (CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(user.Password)))
                {
                    // Activity Log — success
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
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("PasswordChange", user.PasswordChange.ToString())
                        },
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity));
                    return RedirectToAction("Index", "Home");
                }
            }

            // Activity Log — failure (wrong password, or username doesn't exist)
            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = username ?? "Unknown",
                Date = DateTime.Now,
                ActivityAction = ActivityAction.SignInFailed.ToString(),
                Details = $"Failed sign-in attempt for '{username}'."
            });
            await _context.SaveChangesAsync();

            ViewBag.ErrorMessage = "Invalid username or password.";
            return View();
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

    }
}
