using GCAMS.Data;
using GCAMS.Models.Users;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


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

            ViewBag.ErrorMessage = "Invalid username or password.";
            return View();
        }


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
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
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

            // Re-sign-in with an updated "PasswordChange" claim — otherwise the cookie
            // still says "false" until they log out/in again, and the middleware
            // above would keep bouncing them back here.
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
