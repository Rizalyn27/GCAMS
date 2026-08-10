using GCAMS.Data;
using GCAMS.Models;
using GCAMS.Models.Notifs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GCAMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;   // ← was missing

        public HomeController(ILogger<HomeController> logger, AppDbContext context)   // ← add param
        {
            _logger = logger;
            _context = context;   // ← add assignment
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            var unreadAnnouncements = await _context.Notifs
                .Where(n => n.RecipientUsername == username
                         && n.Type == NotificationType.Announcement
                         && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.UnreadAnnouncements = unreadAnnouncements;
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}