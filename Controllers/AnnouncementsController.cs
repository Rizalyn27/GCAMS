using GCAMS.Data;
using GCAMS.Models.ActivityLog;
using GCAMS.Models.Announcements;
using GCAMS.Models.Notifs;
using GCAMS.ViewModels;
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
        // GET: Announcements
        public async Task<IActionResult> Index(int? year, int? month, int? day)
        {
            var today = DateTime.Today;
            int y = year ?? today.Year;
            int m = month ?? today.Month;

            var firstOfMonth = new DateTime(y, m, 1);
            var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

            var monthAnnouncements = await _context.Announcements
                .Include(a => a.Counselor)
                .Where(a => a.CreatedAt.Date >= firstOfMonth && a.CreatedAt.Date <= lastOfMonth)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var countsByDay = monthAnnouncements
                .GroupBy(a => a.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            // Build a full grid: pad out to the Sunday before the 1st, and the Saturday after the last day
            var gridStart = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
            var gridEnd = lastOfMonth.AddDays(6 - (int)lastOfMonth.DayOfWeek);

            var days = new List<CalendarDay>();
            for (var d = gridStart; d <= gridEnd; d = d.AddDays(1))
            {
                days.Add(new CalendarDay
                {
                    Date = d,
                    IsCurrentMonth = d.Month == m,
                    IsToday = d == today,
                    Count = countsByDay.TryGetValue(d, out var c) ? c : 0
                });
            }

            List<Announcement> selectedDayAnnouncements = new();
            DateTime? selectedDate = null;

            if (day.HasValue)
            {
                selectedDate = new DateTime(y, m, day.Value);
                selectedDayAnnouncements = monthAnnouncements
                    .Where(a => a.CreatedAt.Date == selectedDate.Value)
                    .ToList();
            }

            var vm = new AnnouncementCalendarViewModel
            {
                Year = y,
                Month = m,
                MonthName = firstOfMonth.ToString("MMMM yyyy"),
                Days = days,
                SelectedDate = selectedDate,
                SelectedDayAnnouncements = selectedDayAnnouncements
            };

            return View(vm);
        }

        // GET: Announcements/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Message")] Announcement announcement)
        {
            if (!ModelState.IsValid) return View(announcement);

            announcement.CounselorID = await GetCurrentCounselorIdAsync();
            announcement.CreatedAt = DateTime.Now;
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            // Dedup by StuID in case of duplicate student records — otherwise the same
            // student could get two Notifs rows added in the same batch, which the
            // unique index rejects (and takes the whole batch down with it).
            var students = await _context.Students
                .GroupBy(s => s.StuID)
                .Select(g => g.First())
                .ToListAsync();

            int sentCount = 0;

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

                // Save one at a time so a single collision doesn't roll back everyone else.
                try
                {
                    var counselorName = await _context.Counselors
                                        .Where(c => c.EmailAddress == User.Identity.Name)
                                        .Select(c => c.CounselorName)
                                        .FirstOrDefaultAsync();

                    _context.ActivityLog.Add(new ActivityLog
                    {
                        Who = counselorName ?? "Unknown",
                        Date = DateTime.Now,
                        ActivityAction = "AnnouncementCreated",
                        Details = $"{counselorName ?? User.Identity.Name} posted an announcement."
                    });

                    await _context.SaveChangesAsync();
                    sentCount++;
                }
                catch (DbUpdateException)
                {
                    _context.ChangeTracker.Clear();
                }
            }

            TempData["Success"] = $"Announcement sent to {sentCount} student(s).";
            return RedirectToAction(nameof(Index));
        }

        // GET: Announcements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            return View(announcement);
        }

        // POST: Announcements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AnnouncementId,Title,Message,CreatedAt")] Announcement posted)
        {
            if (id != posted.AnnouncementId) return NotFound();

            var existing = await _context.Announcements.FindAsync(id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid) return View(posted);

            existing.Title = posted.Title;
            existing.Message = posted.Message;
            existing.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnnouncementExists(id)) return NotFound();
                throw;
            }

            TempData["Success"] = "Announcement updated.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Announcements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements
                .Include(a => a.Counselor)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);
            if (announcement == null) return NotFound();

            return View(announcement);
        }

        // POST: Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Announcement deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AnnouncementExists(int id)
        {
            return _context.Announcements.Any(e => e.AnnouncementId == id);
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