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

            // ==========================================
            // ALL ANNOUNCEMENTS
            // ==========================================
            var allAnnouncements = await _context.Announcements
                .Include(a => a.Counselor)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.AllAnnouncements = allAnnouncements;


            // ==========================================
            // ALL NOTIFICATIONS FOR CURRENT USER
            // ==========================================
            var allNotifications = await _context.Notifs
                .Where(n => n.RecipientUsername == username
                && n.Type != NotificationType.Announcement)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.AllNotifications = allNotifications;


            // ==========================================
            // UNREAD ANNOUNCEMENTS FOR POPUP
            // ==========================================
            var unreadAnnouncements = await _context.Notifs
                .Where(n => n.RecipientUsername == username
                         && n.Type == NotificationType.Announcement
                         && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.UnreadAnnouncements = unreadAnnouncements;


            // ==========================================
            // ROLE DASHBOARD
            // ==========================================
            if (User.IsInRole("Admin"))
            {
                ViewBag.AdminDashboard = await BuildAdminDashboardAsync();
            }
            else if (User.IsInRole("Counselor"))
            {
                ViewBag.CounselorDashboard =
                    await BuildCounselorDashboardAsync(username, academicYear, month);
            }
            else if (User.IsInRole("Student"))
            {
                ViewBag.StudentDashboard = await BuildStudentDashboardAsync(username);
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

            vm.OpenCases = await _context.Appointments
                .CountAsync(a => a.AppointmentType == "Follow-up Session"
                              && a.Status != "Completed" && a.Status != "Cancelled");

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


        private async Task<CounselorDashboardViewModel> BuildCounselorDashboardAsync(
            string? username, string? academicYear, int? month)
        {
            var vm = new CounselorDashboardViewModel();
            var now = DateTime.Now;
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var weekStart = StartOfWeek(today);
            var weekEnd = weekStart.AddDays(7);

            var counselor = await _context.Counselors
                .FirstOrDefaultAsync(c => c.EmailAddress == username);

            if (counselor == null) return vm;

            var counselorId = counselor.CounselorID;
            vm.CounselorName = $"{counselor.CounselorName}".Trim();

            // ------------------------------------------------------------------
            // Today's schedule
            // ------------------------------------------------------------------
            vm.TodaysAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow
                         && (a.CounselorID == counselorId || a.CounselorID == null)
                         && a.Status == "Confirmed")
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new TodayAppointmentItem
                {
                    AppointmentId = a.AppointmentID,
                    StudentsID = a.StudentsID,
                    FullName = a.FullName,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentType = a.AppointmentType,
                    Status = a.Status,
                    IsUnassigned = a.CounselorID == null
                })
                .ToListAsync();

                        vm.SessionsTodayCount = vm.TodaysAppointments.Count;

            //


            vm.CompletedAppointmentsCount = await _context.Appointments
                .CountAsync(a => (a.CounselorID == counselorId || a.CounselorID == null)
                  && a.Status == "Completed");

            // ------------------------------------------------------------------
            // Baseline: average sessions logged per working day, last 30 days
            // ------------------------------------------------------------------
            var thirtyDaysAgo = today.AddDays(-30);

            var recentSessionCount = await _context.CaseNotes
                .CountAsync(c => c.CounselorID == counselorId
                              && c.SessionDate >= thirtyDaysAgo && c.SessionDate < tomorrow);

            var workingDays = 0;
            for (var d = thirtyDaysAgo; d < today; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    workingDays++;
            }

            vm.AvgSessionsPerDay = workingDays > 0
                ? Math.Round((double)recentSessionCount / workingDays, 1)
                : 0;

            // ------------------------------------------------------------------
            // Pending requests + how long the oldest has waited
            // ------------------------------------------------------------------
            var pending = await _context.Appointments
                .Where(a => a.Status == "Pending"
                         && (a.CounselorID == counselorId || a.CounselorID == null))
                .Select(a => a.CreatedAt)
                .ToListAsync();

            vm.PendingRequestsCount = pending.Count;
            vm.OldestPendingDays = pending.Count > 0
                ? (int)(today - pending.Min().Date).TotalDays
                : 0;

            // ------------------------------------------------------------------
            // Flagged follow-ups
            //
            // NOTE: AppointmentStatusService flips past-dated Pending/Confirmed rows to
            // "Missed" every 3 hours, so "Missed" MUST be included here or this panel is
            // permanently empty. A follow-up leaves this list only when a counselor
            // explicitly marks it Completed or Cancelled.
            // ------------------------------------------------------------------
            var flagged = await _context.Appointments
                .Where(a => (a.CounselorID == counselorId || a.CounselorID == null)
                         && a.AppointmentType == "Follow-up Session"
                         && a.AppointmentDate < now
                         && (a.Status == "Missed" || a.Status == "Pending" || a.Status == "Confirmed"))
                .OrderBy(a => a.AppointmentDate)
                .Take(10)
                .Select(a => new FlaggedCaseItem
                {
                    AppointmentId = a.AppointmentID,
                    StudentsID = a.StudentsID,
                    StudentName = a.FullName,
                    FollowUpDate = a.AppointmentDate,
                    Status = a.Status
                })
                .ToListAsync();

            foreach (var f in flagged)
                f.DaysOverdue = Math.Max(0, (int)(today - f.FollowUpDate.Date).TotalDays);

            vm.FlaggedCases = flagged;
            vm.MostOverdueDays = flagged.Count > 0 ? flagged.Max(f => f.DaysOverdue) : 0;

            // ------------------------------------------------------------------
            // Students drifting: last session >30 days ago, nothing scheduled ahead
            // ------------------------------------------------------------------
            var driftCutoff = today.AddDays(-30);

            var lastSessions = await _context.CaseNotes
                .Where(c => c.CounselorID == counselorId)
                .GroupBy(c => c.StudentsID)
                .Select(g => new
                {
                    StudentsID = g.Key,
                    LastSession = g.Max(c => c.SessionDate),
                    SessionCount = g.Count()
                })
                .Where(x => x.LastSession < driftCutoff)
                .ToListAsync();

            var scheduledAhead = await _context.Appointments
                .Where(a => a.AppointmentDate >= now
                         && a.StudentsID != null
                         && (a.Status == "Pending" || a.Status == "Confirmed"))
                .Select(a => a.StudentsID!.Value)
                .Distinct()
                .ToListAsync();

            var driftIds = lastSessions
                .Select(x => x.StudentsID)
                .Except(scheduledAhead)
                .ToList();

            var driftStudents = await _context.Students
                .Where(s => driftIds.Contains(s.StudentsID) && s.IsActive)
                .Select(s => new { s.StudentsID, s.StuName, s.GradeLevel, s.Section })
                .ToListAsync();

            vm.DriftingStudents = driftStudents
                .Join(lastSessions, s => s.StudentsID, l => l.StudentsID, (s, l) => new DriftingStudentItem
                {
                    StudentsID = s.StudentsID,
                    StudentName = s.StuName,
                    GradeSection = $"{s.GradeLevel} - {s.Section}",
                    LastSessionDate = l.LastSession,
                    DaysSinceLastSession = (int)(today - l.LastSession.Date).TotalDays,
                    SessionCount = l.SessionCount
                })
                .OrderByDescending(d => d.DaysSinceLastSession)
                .Take(5)
                .ToList();

            // ------------------------------------------------------------------
            // Weekly session load (Mon..Sun)
            // ------------------------------------------------------------------
            var weekCaseNoteDates = await _context.CaseNotes
                .Where(c => c.CounselorID == counselorId
                         && c.SessionDate >= weekStart && c.SessionDate < weekEnd)
                .Select(c => c.SessionDate)
                .ToListAsync();

            var loadCounts = new int[7];
            foreach (var d in weekCaseNoteDates)
                loadCounts[(int)((7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7)]++;
            vm.WeeklySessionLoad = loadCounts;

            // ------------------------------------------------------------------
            // Recent observations (anecdotal records)
            //
            // AnecRecs has no CounselorID, so these are school-wide rather than
            // filtered to this counselor. Repeat flag = 2+ records in the last 60 days.
            // ------------------------------------------------------------------
            var sixtyDaysAgo = today.AddDays(-60);

            var repeatStudentIds = await _context.AnecRecs
                .Where(a => a.DateOfObserv >= sixtyDaysAgo)
                .GroupBy(a => a.StudentsID)
                .Where(g => g.Count() >= 2)
                .Select(g => g.Key)
                .ToListAsync();

            var observations = await _context.AnecRecs
                .OrderByDescending(a => a.DateOfObserv)
                .Take(5)
                .Select(a => new ObservationItem
                {
                    AnecRecId = a.AnecRecsId,
                    StudentsID = a.StudentsID,
                    StudentName = a.StuName ?? string.Empty,
                    DateOfObserv = a.DateOfObserv,
                    Place = a.Place,
                    Behavior = a.StudentBehavior
                })
                .ToListAsync();

            foreach (var o in observations)
                o.IsRepeat = repeatStudentIds.Contains(o.StudentsID);

            vm.RecentObservations = observations;

            // ------------------------------------------------------------------
            // Student picker for quick actions
            // ------------------------------------------------------------------
            vm.StudentPicker = await _context.Students
                .Where(s => s.IsActive)
                .OrderBy(s => s.StuName)
                .Select(s => new StudentPickerOption
                {
                    StudentsID = s.StudentsID,
                    Label = s.StuName + " (" + s.StuID + ")"
                })
                .ToListAsync();

            // ------------------------------------------------------------------
            // Session history + top concerns: both driven by the AY/month filter
            // ------------------------------------------------------------------
            var currentAyStart = CurrentAcademicYearStartYear(today);
            var ayStartYear = ParseAcademicYearStartYear(academicYear, currentAyStart);

            vm.AcademicYearOptions = Enumerable.Range(currentAyStart - 2, 6)
                .Select(y => $"{y}-{y + 1}")
                .ToList();
            vm.SelectedAcademicYear = $"{ayStartYear}-{ayStartYear + 1}";
            vm.SelectedMonth = month is >= 1 and <= 12 ? month : null;

            var ayStart = new DateTime(ayStartYear, 6, 1);
            var ayEnd = ayStart.AddYears(1);

            DateTime periodStart, periodEnd;

            if (vm.SelectedMonth.HasValue)
            {
                int calendarYear = vm.SelectedMonth.Value >= 6 ? ayStartYear : ayStartYear + 1;
                periodStart = new DateTime(calendarYear, vm.SelectedMonth.Value, 1);
                periodEnd = periodStart.AddMonths(1);
                vm.PeriodLabel = periodStart.ToString("MMMM yyyy");

                var monthNotes = await _context.CaseNotes
                    .Where(c => c.CounselorID == counselorId
                             && c.SessionDate >= periodStart && c.SessionDate < periodEnd)
                    .Select(c => c.SessionDate)
                    .ToListAsync();

                vm.SessionsInPeriod = monthNotes.Count;

                var weekBuckets = new int[5];
                foreach (var d in monthNotes)
                    weekBuckets[Math.Min(4, (d.Day - 1) / 7)]++;

                vm.PeriodChartLabels = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4", "Week 5" };
                vm.PeriodChartCounts = weekBuckets.ToList();
            }
            else
            {
                periodStart = ayStart;
                periodEnd = ayEnd;
                vm.PeriodLabel = $"AY {vm.SelectedAcademicYear}";

                var ayNotes = await _context.CaseNotes
                    .Where(c => c.CounselorID == counselorId
                             && c.SessionDate >= ayStart && c.SessionDate < ayEnd)
                    .Select(c => c.SessionDate)
                    .ToListAsync();

                vm.SessionsInPeriod = ayNotes.Count;

                var monthBuckets = new int[12];
                var monthLabels = new List<string>();
                for (int i = 0; i < 12; i++)
                    monthLabels.Add(ayStart.AddMonths(i).ToString("MMM"));

                foreach (var d in ayNotes)
                {
                    int offset = ((d.Year - ayStart.Year) * 12) + d.Month - ayStart.Month;
                    if (offset >= 0 && offset < 12) monthBuckets[offset]++;
                }

                vm.PeriodChartLabels = monthLabels;
                vm.PeriodChartCounts = monthBuckets.ToList();
            }

            // Top concerns over the same period.
            // Requires the ConcernCategory column on CaseNotes — see MIGRATION-NOTES.md.
            vm.TopConcerns = await _context.CaseNotes
                .Where(c => c.CounselorID == counselorId
                         && c.SessionDate >= periodStart && c.SessionDate < periodEnd
                         && c.ConcernCategory != null && c.ConcernCategory != "")
                .GroupBy(c => c.ConcernCategory)
                .Select(g => new ConcernCountItem { Category = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(6)
                .ToListAsync();

            return vm;
        }


        private async Task<StudentDashboardViewModel> BuildStudentDashboardAsync(string? username)
        {
            var vm = new StudentDashboardViewModel();
            var now = DateTime.Now;

            // Students sign in with their StuID — same lookup Appointments uses.
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == username);

            if (student == null)
                return vm;   // view renders a safe "no record linked" state

            vm.StudentName = student.StuName ?? string.Empty;

            vm.GradeSection = string.Join(" · ", new[] { student.GradeLevel, student.Section }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            vm.Greeting = now.Hour < 12 ? "Good morning"
                        : now.Hour < 18 ? "Good afternoon"
                                        : "Good evening";

            // ------------------------------------------------------------------
            // Next appointment — the one thing a student opens this page for.
            // Confirmed and Pending both count: a pending request is still the
            // next thing happening, it just needs a different message.
            // ------------------------------------------------------------------
            var next = await _context.Appointments
                .Where(a => a.StudentsID == student.StudentsID
                         && a.AppointmentDate >= now
                         && (a.Status == "Confirmed" || a.Status == "Pending"))
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new UpcomingAppointmentItem
                {
                    AppointmentId = a.AppointmentID,
                    AppointmentType = a.AppointmentType,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                })
                .FirstOrDefaultAsync();

            if (next != null)
            {
                // Computed after materialising — EF can't translate date subtraction.
                next.DaysUntil = (int)(next.AppointmentDate.Date - now.Date).TotalDays;

                var created = await _context.Appointments
                    .Where(a => a.AppointmentID == next.AppointmentId)
                    .Select(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                next.DaysWaiting = (int)(now.Date - created.Date).TotalDays;

                vm.UpcomingAppointment = next;
                vm.HasPendingRequest = next.Status == "Pending";
            }

            // ------------------------------------------------------------------
            // Small history counts — context for the greeting, not a metric.
            // ------------------------------------------------------------------
            vm.TotalAppointments = await _context.Appointments
                .CountAsync(a => a.StudentsID == student.StudentsID);

            vm.CompletedSessions = await _context.Appointments
                .CountAsync(a => a.StudentsID == student.StudentsID && a.Status == "Completed");

            // ------------------------------------------------------------------
            // Announcements — newest three. "View all" drops to the full feed
            // further down the same page.
            // ------------------------------------------------------------------
            vm.RecentAnnouncements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .Select(a => new StudentAnnouncementItem
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Message = a.Message,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            // Anything from the last 3 days gets a "New" tag.
            var newCutoff = now.Date.AddDays(-3);
            foreach (var a in vm.RecentAnnouncements)
                a.IsNew = a.CreatedAt >= newCutoff;

            vm.UnreadAnnouncementCount = await _context.Notifs
                .CountAsync(n => n.RecipientUsername == username
                              && n.Type == NotificationType.Announcement
                              && !n.IsRead);

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