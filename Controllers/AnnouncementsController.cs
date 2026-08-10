using GCAMS.Data;
using GCAMS.Models.Announcements;
using GCAMS.Models.Notifs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCAMS.Controllers
{
    [Authorize(Roles = "Counselor")]
    public class AnnouncementsController : Controller
    {
        private readonly AppDbContext _context;

        public AnnouncementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Announcements
        public async Task<IActionResult> Index()
        {
            var announcements = await _context.Announcements
                .Include(a => a.Counselor)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(announcements);
        }

        // GET: Announcements/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Message,GradeLevel,Section")] Announcement announcement)
        {
            if (!ModelState.IsValid) return View(announcement);

            announcement.CounselorID = await GetCurrentCounselorIdAsync();
            announcement.CreatedAt = DateTime.Now;

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            // Send to every matching student — blank GradeLevel/Section = everyone
            var students = await _context.Students
                .Where(s =>
                    (string.IsNullOrEmpty(announcement.GradeLevel) || s.GradeLevel == announcement.GradeLevel) &&
                    (string.IsNullOrEmpty(announcement.Section) || s.Section == announcement.Section))
                .ToListAsync();

            foreach (var student in students)
            {
                _context.Notifs.Add(new Notifs
                {
                    RecipientUsername = student.StuID,
                    Type = NotificationType.Announcement,
                    Title = announcement.Title,
                    Message = announcement.Message,
                    RelatedEntityType = "Announcement",
                    RelatedEntityId = announcement.AnnouncementId
                });
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Announcement sent to {students.Count} student(s).";
            return RedirectToAction(nameof(Index));
        }

        private async Task<int?> GetCurrentCounselorIdAsync()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _context.Counselors
                .Where(c => c.EmailAddress == email)
                .Select(c => (int?)c.CounselorID)
                .FirstOrDefaultAsync();
        }
    }
}