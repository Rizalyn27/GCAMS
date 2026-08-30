using GCAMS.Data;
using GCAMS.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;

namespace GCAMS.Services
{
    /// <summary>
    /// All report queries live here so ReportsController stays thin and the
    /// same numbers can be reused by an export or a future API.
    /// </summary>
    public class ReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        // =============================================================
        // Shared helpers
        // =============================================================

        /// <summary>School year runs June to May, matching the dashboards.</summary>
        public static int CurrentAcademicYearStartYear()
        {
            var now = DateTime.Now;
            return now.Month >= 6 ? now.Year : now.Year - 1;
        }

        public static int ParseAcademicYearStartYear(string? academicYear)
        {
            if (!string.IsNullOrWhiteSpace(academicYear))
            {
                var head = academicYear.Split('-')[0].Trim();
                if (int.TryParse(head, out var year)) return year;
            }
            return CurrentAcademicYearStartYear();
        }

        /// <summary>Turns the filter into a concrete [start, end) window.</summary>
        public static (DateTime Start, DateTime End, string Label) ResolvePeriod(ReportFilter filter)
        {
            var startYear = ParseAcademicYearStartYear(filter.AcademicYear);
            var ayStart = new DateTime(startYear, 6, 1);
            var ayEnd = ayStart.AddYears(1);

            if (filter.Month is >= 1 and <= 12)
            {
                // June-December sit in the first calendar year, January-May the second.
                var year = filter.Month >= 6 ? startYear : startYear + 1;
                var monthStart = new DateTime(year, filter.Month.Value, 1);

                return (monthStart,
                        monthStart.AddMonths(1),
                        $"{monthStart:MMMM yyyy}");
            }

            return (ayStart, ayEnd, $"AY {startYear}-{startYear + 1}");
        }

        public async Task PopulateOptionsAsync(ReportFilter filter, bool canChooseCounselor)
        {
            filter.CanChooseCounselor = canChooseCounselor;

            var current = CurrentAcademicYearStartYear();
            filter.AcademicYearOptions = Enumerable.Range(0, 5)
                .Select(i => $"{current - i}-{current - i + 1}")
                .ToList();

            if (string.IsNullOrWhiteSpace(filter.AcademicYear))
                filter.AcademicYear = filter.AcademicYearOptions.First();

            filter.GradeLevelOptions = await _context.Students
                .Where(s => s.IsActive && s.GradeLevel != "")
                .Select(s => s.GradeLevel)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            if (canChooseCounselor)
            {
                var counselors = await _context.Counselors
                    .OrderBy(c => c.CounselorName)
                    .Select(c => new { c.CounselorID, c.CounselorName })
                    .ToListAsync();

                filter.CounselorOptions = counselors
                    .Select(c => (c.CounselorID, c.CounselorName))
                    .ToList();
            }
        }

        /// <summary>Resolves the signed-in counselor. Null for admins.</summary>
        public async Task<int?> GetCounselorIdAsync(string? username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            return await _context.Counselors
                .Where(c => c.EmailAddress == username)
                .Select(c => (int?)c.CounselorID)
                .FirstOrDefaultAsync();
        }

        private static void FillMeta(ReportBase report, ReportFilter filter,
                                    string periodLabel, string generatedBy)
        {
            report.Filter = filter;
            report.PeriodLabel = periodLabel;
            report.GeneratedAt = DateTime.Now;
            report.GeneratedBy = generatedBy;
        }

        // =============================================================
        // 1. Counseling Summary
        // =============================================================
        public async Task<CounselingSummaryReport> CounselingSummaryAsync(
            ReportFilter filter, int? scopeCounselorId, string generatedBy)
        {
            var (start, end, label) = ResolvePeriod(filter);

            var report = new CounselingSummaryReport
            {
                Title = "Counseling Summary Report",
                Subtitle = "Sessions logged in the selected period, grouped by concern category.",
                DecisionLine = "When one category dominates the load, that points to a group " +
                               "intervention rather than repeated one-to-one sessions. Anything " +
                               "above 30% of total sessions is flagged.",
                SourceNote = "Source: CaseNotes grouped by ConcernCategory"
            };

            FillMeta(report, filter, label, generatedBy);

            var query = _context.CaseNotes
                .Where(c => c.SessionDate >= start && c.SessionDate < end
                         && c.ConcernCategory != "");

            if (scopeCounselorId.HasValue)
                query = query.Where(c => c.CounselorID == scopeCounselorId.Value);
            else if (filter.CounselorId.HasValue)
                query = query.Where(c => c.CounselorID == filter.CounselorId.Value);

            if (!string.IsNullOrWhiteSpace(filter.GradeLevel))
                query = query.Where(c => c.Student.GradeLevel == filter.GradeLevel);

            // Pull the shape we need, then aggregate in memory — the row count
            // here is sessions in one period, which stays small.
            var raw = await query
                .Select(c => new
                {
                    c.ConcernCategory,
                    c.StudentsID,
                    Grade = c.Student.GradeLevel
                })
                .ToListAsync();

            report.TotalSessions = raw.Count;

            var firstTimers = await query
                .Select(c => c.StudentsID)
                .Distinct()
                .CountAsync();

            report.Rows = raw
                .GroupBy(r => r.ConcernCategory)
                .Select(g =>
                {
                    var topGrade = g.GroupBy(x => x.Grade)
                                    .OrderByDescending(x => x.Count())
                                    .FirstOrDefault();

                    return new ConcernRow
                    {
                        Category = g.Key,
                        Sessions = g.Count(),
                        Students = g.Select(x => x.StudentsID).Distinct().Count(),
                        TopGradeLevel = topGrade?.Key ?? "—",
                        TopGradeCount = topGrade?.Count() ?? 0,
                        SharePercent = report.TotalSessions == 0
                            ? 0
                            : Math.Round(g.Count() * 100.0 / report.TotalSessions, 1)
                    };
                })
                .OrderByDescending(r => r.Sessions)
                .ToList();

            if (report.Rows.Any())
                report.Rows.First().IsLead = true;

            var lead = report.Rows.FirstOrDefault();
            var weeks = Math.Max(1, (end - start).TotalDays / 7.0);

            report.Kpis = new List<ReportKpi>
            {
                new() { Label = "Total sessions", Value = report.TotalSessions.ToString("N0"), Note = label },
                new() { Label = "Students seen",  Value = firstTimers.ToString("N0") },
                new()
                {
                    Label = "Top concern",
                    Value = lead?.Category ?? "—",
                    Note = lead == null ? "No sessions yet" : $"{lead.SharePercent}% of all sessions",
                    //IsWarning = lead != null && lead.SharePercent >= 30
                },
               
            };

            report.SourceNote += $" · {report.TotalSessions} rows";
            return report;
        }

        // =============================================================
        // 2. Follow-up Compliance
        // =============================================================
        public async Task<FollowUpComplianceReport> FollowUpComplianceAsync(
            ReportFilter filter, int? scopeCounselorId, string generatedBy)
        {
            var (start, end, label) = ResolvePeriod(filter);
            var today = DateTime.Today;
            var now = DateTime.Now;

            var report = new FollowUpComplianceReport
            {
                Title = "Follow-up Compliance Report",
                Subtitle = "Follow-up sessions whose date has passed and that were never marked completed.",
                DecisionLine = "Each row is a student who was promised another session and did not " +
                               "get one. Act on it or close it — these do not resolve themselves.",
                SourceNote = "Source: Appointments where AppointmentType = \"Follow-up Session\""
            };

            FillMeta(report, filter, label, generatedBy);

            var scheduled = _context.Appointments
                .Where(a => a.AppointmentType == "Follow-up Session"
                         && a.AppointmentDate >= start && a.AppointmentDate < end);

            if (scopeCounselorId.HasValue)
            {
                scheduled = scheduled.Where(a => a.CounselorID == scopeCounselorId.Value
                                              || a.CounselorID == null);
            }
            else if (filter.CounselorId.HasValue)
            {
                scheduled = scheduled.Where(a => a.CounselorID == filter.CounselorId.Value);
            }

            report.TotalScheduled = await scheduled.CountAsync();

            // Overdue: date has passed and it was never completed or called off.
            // Missed matters most — AppointmentStatusService flips past-dated
            // Pending/Confirmed rows to Missed every 3 hours, so excluding it
            // would hide almost everything.
            var overdueQuery = scheduled
                .Where(a => a.AppointmentDate < now
                         && (a.Status == "Missed" || a.Status == "Pending" || a.Status == "Confirmed"));

            var rows = await overdueQuery
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new FollowUpRow
                {
                    AppointmentId = a.AppointmentID,
                    StudentsID = a.StudentsID,
                    StudentName = a.Student != null ? a.Student.StuName : a.FullName,
                    GradeSection = a.Student != null
                        ? a.Student.GradeLevel + " — " + a.Student.Section
                        : "",
                    DueDate = a.AppointmentDate,
                    Status = a.Status,
                    CounselorName = a.Counselor != null ? a.Counselor.CounselorName : "Unassigned"
                })
                .ToListAsync();

            foreach (var r in rows)
                r.DaysOverdue = (int)(today - r.DueDate.Date).TotalDays;

            if (filter.MinDaysOverdue is > 0)
                rows = rows.Where(r => r.DaysOverdue >= filter.MinDaysOverdue.Value).ToList();

            // Session counts, one query for the whole page rather than per row.
            var ids = rows.Where(r => r.StudentsID.HasValue)
                          .Select(r => r.StudentsID!.Value)
                          .Distinct()
                          .ToList();

            var counts = await _context.CaseNotes
                .Where(c => ids.Contains(c.StudentsID))
                .GroupBy(c => c.StudentsID)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            var lookup = counts.ToDictionary(x => x.Key, x => x.Count);

            foreach (var r in rows)
                if (r.StudentsID.HasValue && lookup.TryGetValue(r.StudentsID.Value, out var n))
                    r.SessionCount = n;

            report.Rows = rows.OrderByDescending(r => r.DaysOverdue).ToList();
            report.TotalOverdue = rows.Count;

            report.CompliancePercent = report.TotalScheduled == 0
                ? 100
                : Math.Round((report.TotalScheduled - report.TotalOverdue)
                             * 100.0 / report.TotalScheduled, 1);

            var worst = rows.OrderByDescending(r => r.DaysOverdue).FirstOrDefault();

            var resolved = await scheduled
                .CountAsync(a => a.Status == "Completed");

            report.Kpis = new List<ReportKpi>
            {
                new()
                {
                    Label = "Overdue follow-ups",
                    Value = report.TotalOverdue.ToString("N0"),
                    Note = $"of {report.TotalScheduled} scheduled",
                    //IsWarning = report.TotalOverdue > 0
                },
                
                new()
                {
                    Label = "Longest overdue",
                    Value = worst == null ? "—" : $"{worst.DaysOverdue} day(s)",
                    Note = worst?.StudentName,
                    IsWarning = worst != null && worst.DaysOverdue > 14
                },
                new() { Label = "Completed", Value = resolved.ToString("N0"), Note = label }
            };

            report.SourceNote += $" and status is Missed, Pending or Confirmed · {report.TotalOverdue} rows";
            return report;
        }

        // =============================================================
        // 3. Drifting Students
        // =============================================================
        public async Task<DriftingStudentsReport> DriftingStudentsAsync(
            ReportFilter filter, int? scopeCounselorId, string generatedBy)
        {
            var today = DateTime.Today;
            var now = DateTime.Now;
            var threshold = filter.DriftThresholdDays <= 0 ? 30 : filter.DriftThresholdDays;
            var cutoff = today.AddDays(-threshold);

            var report = new DriftingStudentsReport
            {
                Title = "Drifting Students Report",
                Subtitle = $"Students with counselling history whose last session was over " +
                           $"{threshold} days ago and who have nothing scheduled.",
                DecisionLine = "Nobody promised these students a follow-up, so they appear " +
                               "nowhere else in the system. This report is the only thing that " +
                               "surfaces them.",
                SourceNote = "Source: CaseNotes grouped by student, excluding anyone with a " +
                             "Pending or Confirmed appointment ahead"
            };

            FillMeta(report, filter, $"Threshold {threshold} days", generatedBy);

            var notes = _context.CaseNotes.AsQueryable();

            if (scopeCounselorId.HasValue)
                notes = notes.Where(c => c.CounselorID == scopeCounselorId.Value);
            else if (filter.CounselorId.HasValue)
                notes = notes.Where(c => c.CounselorID == filter.CounselorId.Value);

            var lastSessions = await notes
                .GroupBy(c => c.StudentsID)
                .Select(g => new
                {
                    StudentsID = g.Key,
                    LastSession = g.Max(c => c.SessionDate),
                    SessionCount = g.Count()
                })
                .ToListAsync();

            report.StudentsWithHistory = lastSessions.Count;

            var stale = lastSessions.Where(x => x.LastSession < cutoff).ToList();

            // Anyone with something booked ahead is covered, whoever booked it.
            var scheduledAhead = await _context.Appointments
                .Where(a => a.StudentsID != null
                         && a.AppointmentDate >= now
                         && (a.Status == "Pending" || a.Status == "Confirmed"))
                .Select(a => a.StudentsID!.Value)
                .Distinct()
                .ToListAsync();

            var driftIds = stale.Select(x => x.StudentsID)
                                .Except(scheduledAhead)
                                .ToList();

            var studentQuery = _context.Students
                .Where(s => driftIds.Contains(s.StudentsID) && s.IsActive);

            if (!string.IsNullOrWhiteSpace(filter.GradeLevel))
                studentQuery = studentQuery.Where(s => s.GradeLevel == filter.GradeLevel);

            var students = await studentQuery
                .Select(s => new { s.StudentsID, s.StuName, s.GradeLevel, s.Section })
                .ToListAsync();

            // Most recent concern category per student, for context on the row.
            var lastConcerns = await _context.CaseNotes
                .Where(c => driftIds.Contains(c.StudentsID))
                .GroupBy(c => c.StudentsID)
                .Select(g => new
                {
                    g.Key,
                    Concern = g.OrderByDescending(c => c.SessionDate)
                               .Select(c => c.ConcernCategory)
                               .FirstOrDefault()
                })
                .ToListAsync();

            var concernLookup = lastConcerns.ToDictionary(x => x.Key, x => x.Concern ?? "");

            report.Rows = students
                .Join(stale, s => s.StudentsID, l => l.StudentsID, (s, l) => new DriftRow
                {
                    StudentsID = s.StudentsID,
                    StudentName = s.StuName,
                    GradeSection = $"{s.GradeLevel} — {s.Section}",
                    LastSessionDate = l.LastSession,
                    DaysSince = (int)(today - l.LastSession.Date).TotalDays,
                    SessionCount = l.SessionCount,
                    LastConcern = concernLookup.TryGetValue(s.StudentsID, out var c) && c != ""
                        ? c : "—"
                })
                .OrderByDescending(r => r.DaysSince)
                .ToList();

            var worst = report.Rows.FirstOrDefault();

            var reEngaged = lastSessions.Count(x => x.LastSession >= cutoff);

            report.Kpis = new List<ReportKpi>
            {
                new()
                {
                    Label = "Drifting students",
                    Value = report.Rows.Count.ToString("N0"),
                    Note = $"of {report.StudentsWithHistory} with history",
                    IsWarning = report.Rows.Any()
                },
                new()
                {
                    Label = "Longest gap",
                    Value = worst == null ? "—" : $"{worst.DaysSince} days",
                    Note = worst?.StudentName,
                    IsWarning = worst != null && worst.DaysSince > threshold * 2
                },
                new()
                {
                    Label = "Average gap",
                    Value = report.Rows.Any()
                        ? $"{report.Rows.Average(r => r.DaysSince):0} days"
                        : "—",
                    Note = $"threshold {threshold}"
                },
                new()
                {
                    Label = "Seen recently",
                    Value = reEngaged.ToString("N0"),
                    Note = $"within {threshold} days"
                }
            };

            report.SourceNote += $" · {report.Rows.Count} rows";
            return report;
        }

        // =============================================================
        // 4. Behavioral Incidents
        // =============================================================
        public async Task<IncidentReport> IncidentsAsync(
     ReportFilter filter, string generatedBy)
        {
            var (start, end, label) = ResolvePeriod(filter);

            var report = new IncidentReport
            {
                Title = "Behavioral Incident Report",
                Subtitle = "Anecdotal records grouped by student.",
                DecisionLine = "Repeat incidents for the same student point toward a pattern worth " +
                               "a case note, not just a one-off observation.",
                SourceNote = "Source: AnecRecs grouped by Student"
            };

            FillMeta(report, filter, label, generatedBy);

            var query = _context.AnecRecs
                .Where(a => a.DateOfObserv >= start && a.DateOfObserv < end
                         && a.Place != "");

            if (!string.IsNullOrWhiteSpace(filter.GradeLevel))
                query = query.Where(a => a.Student.GradeLevel == filter.GradeLevel);

            var raw = await query
                .Select(a => new
                {
                    a.StudentsID,
                    StudentName = a.Student.StuName,
                    a.Place,
                    a.DateOfObserv
                })
                .ToListAsync();

            report.TotalIncidents = raw.Count;

            // Table rows: one per student, most frequent location as context.
            report.Rows = raw
                .GroupBy(r => new { r.StudentsID, r.StudentName })
                .Select(g =>
                {
                    var topPlace = g.GroupBy(x => x.Place)
                                    .OrderByDescending(x => x.Count())
                                    .FirstOrDefault();

                    return new IncidentRow
                    {
                        StudentsID = g.Key.StudentsID,
                        StudentName = g.Key.StudentName,
                        Place = topPlace?.Key ?? "—",
                        Incidents = g.Count()
                    };
                })
                .OrderByDescending(r => r.Incidents)
                .ToList();

            if (report.Rows.Any())
                report.Rows.First().IsLead = true;

            // KPI strip still reasons about location, computed separately from the table.
            var byLocation = raw
                .GroupBy(r => r.Place)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            var peakMonth = raw
                .GroupBy(r => new { r.DateOfObserv.Year, r.DateOfObserv.Month })
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            var repeatStudents = report.Rows.Count(r => r.Incidents > 1);

            report.Kpis = new List<ReportKpi>
    {
        new() { Label = "Incidents logged", Value = report.TotalIncidents.ToString("N0"), Note = label },
        new()
        {
            Label = "Students involved",
            Value = report.Rows.Count.ToString("N0"),
            Note = $"{repeatStudents} with repeats",
            IsWarning = repeatStudents > 0
        },
        new()
        {
            Label = "Top location",
            Value = byLocation?.Key ?? "—",
            Note = byLocation == null ? "No records yet" : $"{byLocation.Count()} incidents",
        },
        new()
        {
            Label = "Peak month",
            Value = peakMonth == null
                ? "—"
                : new DateTime(peakMonth.Key.Year, peakMonth.Key.Month, 1).ToString("MMMM"),
            Note = peakMonth == null ? null : $"{peakMonth.Count()} incidents"
        }
    };

            report.SourceNote += $" · {report.TotalIncidents} rows";
            return report;
        }

        // =============================================================
        // 5. Appointment Utilisation (admin)
        // =============================================================
        public async Task<UtilisationReport> UtilisationAsync(
            ReportFilter filter, string generatedBy)
        {
            var (start, end, label) = ResolvePeriod(filter);
            var now = DateTime.Now;

            var report = new UtilisationReport
            {
                Title = "Appointment Report",
                Subtitle = "What happens to appointment requests once students submit them.",
                DecisionLine = "A high pending share means students are waiting; a high missed " +
                               "share means the schedule is not working for them. The two need " +
                               "opposite responses.",
                SourceNote = "Source: Appointments grouped by Status, response time measured " +
                             "from CreatedAt to UpdatedAt"
            };

            FillMeta(report, filter, label, generatedBy);

            var query = _context.Appointments
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate < end);

            if (filter.CounselorId.HasValue)
                query = query.Where(a => a.CounselorID == filter.CounselorId.Value);

            var raw = await query
                .Select(a => new
                {
                    a.Status,
                    a.AppointmentDate,
                    a.CreatedAt,
                    a.UpdatedAt,
                    a.StudentsID
                })
                .ToListAsync();

            report.TotalRequests = raw.Count;

            var responded = raw
                .Where(a => a.UpdatedAt.HasValue && a.UpdatedAt > a.CreatedAt)
                .Select(a => (a.UpdatedAt!.Value - a.CreatedAt).TotalHours)
                .ToList();

            report.AvgResponseHours = responded.Any()
                ? Math.Round(responded.Average(), 1)
                : null;

            report.PendingOver48h = raw.Count(a => a.Status == "Pending"
                                               && (now - a.CreatedAt).TotalHours > 48);

            var chronic = raw
                .Where(a => a.Status == "Missed" && a.StudentsID != null)
                .GroupBy(a => a.StudentsID)
                .Count(g => g.Count() >= 2);

            var peak = raw
                .GroupBy(a => a.AppointmentDate.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            report.PeakDay = peak?.Key.ToString() ?? "—";

            var notes = new Dictionary<string, (string Text, bool Warn)>
            {
                ["Completed"] = ("Session took place", false),
                ["Confirmed"] = ("Scheduled ahead", false),
                ["Missed"] = (chronic > 0 ? $"{chronic} students with 2+ misses" : "No repeat pattern", chronic > 0),
                ["Pending"] = (report.PendingOver48h > 0 ? $"{report.PendingOver48h} waiting over 48 hrs" : "All within 48 hrs", report.PendingOver48h > 0),
                ["Cancelled"] = ("Withdrawn by the student", false),
                ["Rejected"] = ("Declined by a counselor", false)
            };

            report.Rows = raw
                .GroupBy(a => a.Status)
                .Select(g => new UtilisationRow
                {
                    Status = g.Key,
                    Count = g.Count(),
                    SharePercent = report.TotalRequests == 0
                        ? 0
                        : Math.Round(g.Count() * 100.0 / report.TotalRequests, 1),
                    Note = notes.TryGetValue(g.Key, out var n) ? n.Text : "",
                    NoteIsWarning = notes.TryGetValue(g.Key, out var w) && w.Warn
                })
                .OrderByDescending(r => r.Count)
                .ToList();

            if (report.Rows.Any())
                report.Rows.First().IsLead = true;

            var missedShare = report.Rows.FirstOrDefault(r => r.Status == "Missed")?.SharePercent ?? 0;

            report.Kpis = new List<ReportKpi>
            {
                new() { Label = "Requests received", Value = report.TotalRequests.ToString("N0"), Note = label },

                new()
                {
                    Label = "Missed Appointments",
                    Value = (report.Rows.FirstOrDefault(r => r.Status == "Missed")?.Count ?? 0).ToString("N0"),
                    //IsWarning = missedShare >= 10
                },
                new() { Label = "Peak day", Value = report.PeakDay }
            };

            report.SourceNote += $" · {report.TotalRequests} rows";
            return report;
        }

        // =============================================================
        // 6. Counselor Workload (admin)
        // =============================================================
        public async Task<WorkloadReport> WorkloadAsync(
            ReportFilter filter, string generatedBy)
        {
            var (start, end, label) = ResolvePeriod(filter);
            var now = DateTime.Now;
            var today = DateTime.Today;

            var report = new WorkloadReport
            {
                Title = "Counselor Workload Report",
                Subtitle = "How casework is distributed across counselors, plus anything unassigned.",
                DecisionLine = "If one counselor carries substantially more open cases than the " +
                               "others, reassign before the backlog turns into overdue follow-ups.",
                SourceNote = "Source: CaseNotes and Appointments grouped by CounselorID, " +
                             "including rows where CounselorID is null"
            };

            FillMeta(report, filter, label, generatedBy);

            var counselors = await _context.Counselors
                .Select(c => new { c.CounselorID, c.CounselorName, c.Position, c.EmploymentStatus })
                .ToListAsync();

            var sessions = await _context.CaseNotes
                .Where(c => c.SessionDate >= start && c.SessionDate < end && c.CounselorID != null)
                .GroupBy(c => c.CounselorID!.Value)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            // Open = a follow-up appointment not yet completed or called off.
            var open = await _context.Appointments
                .Where(a => a.AppointmentType == "Follow-up Session"
                         && a.Status != "Completed" && a.Status != "Cancelled" && a.Status != "Rejected"
                         && a.CounselorID != null)
                .Select(a => new { a.CounselorID, a.AppointmentDate })
                .ToListAsync();

            var sessionLookup = sessions.ToDictionary(x => x.Key, x => x.Count);

            var rows = counselors
                .Where(c => c.EmploymentStatus != "Inactive" && c.EmploymentStatus != "Retired")
                .Select(c => new WorkloadRow
                {
                    CounselorId = c.CounselorID,
                    CounselorName = c.CounselorName,
                    Position = c.Position,
                    Sessions = sessionLookup.TryGetValue(c.CounselorID, out var s) ? s : 0,
                    OpenCases = open.Count(o => o.CounselorID == c.CounselorID),
                    Overdue = open.Count(o => o.CounselorID == c.CounselorID
                                           && o.AppointmentDate < now)
                })
                .ToList();

            var totalOpen = rows.Sum(r => r.OpenCases);

            foreach (var r in rows)
                r.SharePercent = totalOpen == 0
                    ? 0
                    : Math.Round(r.OpenCases * 100.0 / totalOpen, 1);

            rows = rows.OrderByDescending(r => r.OpenCases).ToList();

            if (rows.Any())
                rows.First().IsLead = true;

            report.UnassignedAppointments = await _context.Appointments
                .CountAsync(a => a.CounselorID == null
                              && a.AppointmentDate >= now
                              && a.Status != "Cancelled" && a.Status != "Rejected");

            //if (report.UnassignedAppointments > 0)
            //{
            //    rows.Add(new WorkloadRow
            //    {
            //        CounselorId = null,
            //        CounselorName = "Unassigned",
            //        Position = "No counselor set",
            //        OpenCases = report.UnassignedAppointments
            //    });
            //}

            report.Rows = rows;
            report.ActiveCounselors = rows.Count(r => r.CounselorId.HasValue);

            var activeStudents = await _context.Students.CountAsync(s => s.IsActive);

            report.StudentsPerCounselor = report.ActiveCounselors == 0
                ? 0
                : activeStudents / report.ActiveCounselors;

            var busiest = rows.FirstOrDefault(r => r.CounselorId.HasValue);

            report.Kpis = new List<ReportKpi>
            {
                new()
                {
                    Label = "Active counselors",
                    Value = report.ActiveCounselors.ToString("N0"),
                    IsWarning = report.StudentsPerCounselor > 500
                },
                new()
                {
                    Label = "Busiest",
                    Value = busiest?.CounselorName ?? "—",
                    //IsWarning = busiest != null && busiest.SharePercent >= 50
                },
                new()
                {
                    Label = "Sessions logged",
                    Value = rows.Sum(r => r.Sessions).ToString("N0"),
                    Note = label
                }
            };

            report.SourceNote += $" · {report.Rows.Count} rows";
            return report;
        }

        // =============================================================
        // 7. Activity Log (admin)
        // =============================================================
        public async Task<AuditReport> AuditAsync(
            ReportFilter filter, string generatedBy)
        {
            var from = filter.FromDate ?? DateTime.Today.AddDays(-30);
            var to = (filter.ToDate ?? DateTime.Today).AddDays(1);

            var report = new AuditReport
            {
                Title = "Activity Log Report",
                Subtitle = "Who changed what, and when — the accountability trail behind " +
                           "role-based access control.",
                DecisionLine = "Repeated failed sign-ins, or edits from outside a person's role, " +
                               "are the signal to reset credentials or deactivate the account.",
                SourceNote = "Source: ActivityLogs, newest first"
            };

            FillMeta(report, filter, $"{from:MMM d} – {to.AddDays(-1):MMM d, yyyy}", generatedBy);

            var query = _context.ActivityLogs
                .Where(a => a.Date >= from && a.Date < to);

            if (!string.IsNullOrWhiteSpace(filter.User))
                query = query.Where(a => a.Who == filter.User);

            if (!string.IsNullOrWhiteSpace(filter.Action))
                query = query.Where(a => a.ActivityAction == filter.Action);

            report.TotalEvents = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Date)
                .Take(200)
                .Select(a => new { a.Who, a.Date, a.ActivityAction, a.Details })
                .ToListAsync();

            var whoList = logs.Select(l => l.Who).Distinct().ToList();

            var roles = await _context.Users
                .Where(u => whoList.Contains(u.Username))
                .Select(u => new { u.Username, u.Role })
                .ToListAsync();

            var roleLookup = roles.ToDictionary(r => r.Username, r => r.Role);

            report.Rows = logs.Select(l => new AuditRow
            {
                Who = l.Who,
                Role = roleLookup.TryGetValue(l.Who, out var r) ? r : "—",
                Action = l.ActivityAction,
                Details = l.Details,
                When = l.Date
            }).ToList();

            report.UserOptions = await _context.ActivityLogs
                .Select(a => a.Who).Distinct().OrderBy(w => w).ToListAsync();

            report.ActionOptions = await _context.ActivityLogs
                .Select(a => a.ActivityAction).Distinct().OrderBy(a => a).ToListAsync();

            var failed = logs.Count(l => l.ActivityAction.Contains("Login")
                                      && l.Details.Contains("ailed"));

            var deletes = logs.Count(l => l.ActivityAction == "Delete");
            var updates = logs.Count(l => l.ActivityAction == "Update");

            report.Kpis = new List<ReportKpi>
            {
                new() { Label = "Events logged", Value = report.TotalEvents.ToString("N0"), Note = report.PeriodLabel },
                new()
                {
                    Label = "Failed sign-ins",
                    Value = failed.ToString("N0"),
                    IsWarning = failed > 3
                },
                new() { Label = "Records edited", Value = updates.ToString("N0") },
                new() { Label = "Deletions", Value = deletes.ToString("N0"), IsWarning = deletes > 0 }
            };

            report.SourceNote += $" · showing {report.Rows.Count} of {report.TotalEvents}";
            return report;
        }
    }
}