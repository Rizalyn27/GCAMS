using GCAMS.Data;
using GCAMS.Models;
using GCAMS.Models.Notifs;
using GCAMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GCAMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string? academicYear, int? month)
        {
            var username = User.Identity?.Name;
            var unreadAnnouncements = await _context.Notifs
                .Where(n => n.RecipientUsername == username
                         && n.Type == NotificationType.Announcement
                         && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.UnreadAnnouncements = unreadAnnouncements;

            if (User.IsInRole("Admin"))
            {
                ViewBag.AdminDashboard = await BuildAdminDashboardAsync();
            }
            else if (User.IsInRole("Counselor"))
            {
                ViewBag.CounselorDashboard = await BuildCounselorDashboardAsync(username, academicYear, month);
            }

            return View();
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            // Week starts Monday
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-diff);
        }

        // Philippine public school AY runs June -> May. "2026-2027" means June 1, 2026 through May 31, 2027.
        private static int CurrentAcademicYearStartYear(DateTime date)
        {
            return date.Month >= 6 ? date.Year : date.Year - 1;
        }

        private static int ParseAcademicYearStartYear(string? academicYear, int fallbackStartYear)
        {
            if (!string.IsNullOrWhiteSpace(academicYear))
            {
                var parts = academicYear.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out var parsedYear))
                {
                    return parsedYear;
                }
            }
            return fallbackStartYear;
        }

        private async Task<AdminDashboardViewModel> BuildAdminDashboardAsync()
        {
            var vm = new AdminDashboardViewModel();
            var today = DateTime.Today;
            var weekStart = StartOfWeek(today);
            var weekEnd = weekStart.AddDays(7);

            vm.TotalActiveStudents = await _context.Students.CountAsync(s => s.IsActive);

            vm.ActiveCounselors = await _context.Counselors
                .CountAsync(c => c.EmploymentStatus != "Inactive" && c.EmploymentStatus != "Retired");

            var weekAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= weekStart && a.AppointmentDate < weekEnd)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            vm.AppointmentsThisWeek = weekAppointments.Count;

            var counts = new int[7];
            foreach (var d in weekAppointments)
            {
                int idx = (int)((7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7);
                counts[idx]++;
            }
            vm.WeeklyAppointmentCounts = counts;

            vm.OpenCases = await _context.CaseNotes
                .CountAsync(c => c.FollowUpDate != null && c.FollowUpDate >= today);

            vm.StudentUserCount = await _context.Users.CountAsync(u => u.Role == "Student");
            vm.CounselorUserCount = await _context.Users.CountAsync(u => u.Role == "Counselor");
            vm.AdminUserCount = await _context.Users.CountAsync(u => u.Role == "Admin");

            var recentAppointments = await _context.Appointments
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .Select(a => new ActivityItem
                {
                    Icon = "event_available",
                    Text = $"Appointment booked by {a.FullName}",
                    Timestamp = a.CreatedAt
                })
                .ToListAsync();

            var recentAnnouncements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .Select(a => new ActivityItem
                {
                    Icon = "campaign",
                    Text = $"Announcement posted: {a.Title}",
                    Timestamp = a.CreatedAt
                })
                .ToListAsync();

            vm.RecentActivity = recentAppointments
                .Concat(recentAnnouncements)
                .OrderByDescending(a => a.Timestamp)
                .Take(5)
                .ToList();

            var pendingUsers = await _context.Users
                .Where(u => u.PasswordChange == false && u.IsActive)
                .OrderByDescending(u => u.UserId)
                .Take(5)
                .Select(u => new { u.Username, u.Role })
                .ToListAsync();

            vm.PendingSetupAccounts = pendingUsers
                .Select(u => (u.Username, u.Role))
                .ToList();

            return vm;
        }

        private async Task<CounselorDashboardViewModel> BuildCounselorDashboardAsync(string? username, string? academicYear, int? month)
        {
            var vm = new CounselorDashboardViewModel();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var weekStart = StartOfWeek(today);
            var weekEnd = weekStart.AddDays(7);

            var counselor = await _context.Counselors
                .FirstOrDefaultAsync(c => c.EmailAddress == username);

            if (counselor == null)
            {
                return vm;
            }

            vm.CounselorName = $"{counselor.CounselorName}".Trim();

            var todaysAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow
                         && (a.CounselorID == counselor.CounselorID || a.CounselorID == null)
                         && a.Status != "Cancelled" && a.Status != "Rejected")
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new TodayAppointmentItem
                {
                    AppointmentId = a.AppointmentID,
                    FullName = a.FullName,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentType = a.AppointmentType,
                    Status = a.Status
                })
                .ToListAsync();

            vm.TodaysAppointments = todaysAppointments;
            vm.SessionsTodayCount = todaysAppointments.Count;

            vm.OpenCasesCount = await _context.CaseNotes
                .CountAsync(c => c.CounselorID == counselor.CounselorID
                              && (c.FollowUpDate == null || c.FollowUpDate >= today));

            vm.PendingRequestsCount = await _context.Appointments
                .CountAsync(a => a.Status == "Pending"
                              && (a.CounselorID == counselor.CounselorID || a.CounselorID == null));

            var weekCaseNoteDates = await _context.CaseNotes
                .Where(c => c.CounselorID == counselor.CounselorID
                         && c.SessionDate >= weekStart && c.SessionDate < weekEnd)
                .Select(c => c.SessionDate)
                .ToListAsync();

            var loadCounts = new int[7];
            foreach (var d in weekCaseNoteDates)
            {
                int idx = (int)((7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7);
                loadCounts[idx]++;
            }
            vm.WeeklySessionLoad = loadCounts;

            vm.FlaggedCases = await _context.CaseNotes
                .Where(c => c.CounselorID == counselor.CounselorID
                         && c.FollowUpDate != null && c.FollowUpDate <= today)
                .OrderBy(c => c.FollowUpDate)
                .Take(5)
                .Select(c => new FlaggedCaseItem
                {
                    StudentName = c.FullName,
                    FollowUpDate = c.FollowUpDate!.Value,
                    BehaviorStatus = c.BehaviorStatus
                })
                .ToListAsync();

            // ---- Session history: academic year + month filter ----
            var currentAyStart = CurrentAcademicYearStartYear(today);
            var ayStartYear = ParseAcademicYearStartYear(academicYear, currentAyStart);

            vm.AcademicYearOptions = Enumerable.Range(currentAyStart - 2, 6)
                .Select(y => $"{y}-{y + 1}")
                .ToList();
            vm.SelectedAcademicYear = $"{ayStartYear}-{ayStartYear + 1}";
            vm.SelectedMonth = month is >= 1 and <= 12 ? month : null;

            var ayStart = new DateTime(ayStartYear, 6, 1);
            var ayEnd = ayStart.AddYears(1); // exclusive, May 31 inclusive

            if (vm.SelectedMonth.HasValue)
            {
                // Map the chosen calendar month onto the correct year within the AY (Jun-Dec -> ayStartYear, Jan-May -> ayStartYear+1)
                int calendarYear = vm.SelectedMonth.Value >= 6 ? ayStartYear : ayStartYear + 1;
                var monthStart = new DateTime(calendarYear, vm.SelectedMonth.Value, 1);
                var monthEnd = monthStart.AddMonths(1);
                vm.PeriodLabel = monthStart.ToString("MMMM yyyy");

                var monthNotes = await _context.CaseNotes
                    .Where(c => c.CounselorID == counselor.CounselorID
                             && c.SessionDate >= monthStart && c.SessionDate < monthEnd)
                    .Select(c => c.SessionDate)
                    .ToListAsync();

                vm.SessionsInPeriod = monthNotes.Count;

                var weekBuckets = new int[5]; // Week 1..5 of the month
                foreach (var d in monthNotes)
                {
                    int weekIdx = Math.Min(4, (d.Day - 1) / 7);
                    weekBuckets[weekIdx]++;
                }
                vm.PeriodChartLabels = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4", "Week 5" };
                vm.PeriodChartCounts = weekBuckets.ToList();
            }
            else
            {
                vm.PeriodLabel = $"AY {vm.SelectedAcademicYear}";

                var ayNotes = await _context.CaseNotes
                    .Where(c => c.CounselorID == counselor.CounselorID
                             && c.SessionDate >= ayStart && c.SessionDate < ayEnd)
                    .Select(c => c.SessionDate)
                    .ToListAsync();

                vm.SessionsInPeriod = ayNotes.Count;

                var monthBuckets = new int[12]; // index 0 = June ... 11 = May
                var monthLabels = new List<string>();
                for (int i = 0; i < 12; i++)
                {
                    monthLabels.Add(ayStart.AddMonths(i).ToString("MMM"));
                }
                foreach (var d in ayNotes)
                {
                    int monthsFromAyStart = ((d.Year - ayStart.Year) * 12) + d.Month - ayStart.Month;
                    if (monthsFromAyStart >= 0 && monthsFromAyStart < 12)
                    {
                        monthBuckets[monthsFromAyStart]++;
                    }
                }
                vm.PeriodChartLabels = monthLabels;
                vm.PeriodChartCounts = monthBuckets.ToList();
            }

            return vm;
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