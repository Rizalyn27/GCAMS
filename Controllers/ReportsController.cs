using System.Text;
using GCAMS.Services;
using GCAMS.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GCAMS.Controllers
{
    [Authorize(Roles = "Admin,Counselor")]
    public class ReportsController : Controller
    {
        private readonly ReportService _reports;

        public ReportsController(ReportService reports)
        {
            _reports = reports;
        }

        private bool IsAdmin => User.IsInRole("Admin");
        private string Who => User.Identity?.Name ?? "Unknown";

        /// <summary>
        /// The signed-in counselor's ID, or null for an admin. Passing null
        /// into the service means school-wide; passing an ID scopes every
        /// query to that counselor's own caseload.
        /// </summary>
        private async Task<int?> ScopeAsync()
        {
            if (IsAdmin) return null;
            return await _reports.GetCounselorIdAsync(User.Identity?.Name);
        }

        // =============================================================
        // One page, all reports, tabs on the client.
        // =============================================================
        public async Task<IActionResult> Index(ReportFilter filter, string? tab)
        {
            filter ??= new ReportFilter();
            await _reports.PopulateOptionsAsync(filter, IsAdmin);

            var scope = await ScopeAsync();
            var (_, _, periodLabel) = ReportService.ResolvePeriod(filter);

            var vm = new ReportsPageViewModel
            {
                Filter = filter,
                IsAdmin = IsAdmin,
                PeriodLabel = periodLabel,
                ActiveTab = string.IsNullOrWhiteSpace(tab) ? "summary" : tab,

                Summary = await _reports.CounselingSummaryAsync(filter, scope, Who),
                FollowUp = await _reports.FollowUpComplianceAsync(filter, scope, Who),
                Drift = await _reports.DriftingStudentsAsync(filter, scope, Who),
                Incidents = await _reports.IncidentsAsync(filter, Who)
            };

            if (IsAdmin)
            {
                vm.Utilisation = await _reports.UtilisationAsync(filter, Who);
                vm.Workload = await _reports.WorkloadAsync(filter, Who);
                vm.Audit = await _reports.AuditAsync(filter, Who);
            }

            return View(vm);
        }

        // =============================================================
        // Export — one report at a time, as CSV.
        //
        // No email gateway is in scope, so export means download. CSV needs
        // no extra package and opens straight into Excel.
        // =============================================================
        public async Task<IActionResult> Export(string report, ReportFilter filter)
        {
            filter ??= new ReportFilter();
            await _reports.PopulateOptionsAsync(filter, IsAdmin);

            var scope = await ScopeAsync();

            string[] headers;
            List<string[]> rows;
            string name;

            switch ((report ?? "").ToLowerInvariant())
            {
                case "summary":
                    {
                        var m = await _reports.CounselingSummaryAsync(filter, scope, Who);
                        name = "counseling-summary";
                        headers = new[] { "Concern category", "Sessions", "Students", "Share %", "Most affected grade" };
                        rows = m.Rows.Select(r => new[]
                        {
                        r.Category, r.Sessions.ToString(), r.Students.ToString(),
                        r.SharePercent.ToString(), r.TopGradeLevel
                    }).ToList();
                        break;
                    }

                case "followup":
                    {
                        var m = await _reports.FollowUpComplianceAsync(filter, scope, Who);
                        name = "follow-up-compliance";
                        headers = new[] { "Student", "Grade & section", "Due", "Days overdue", "Status", "Counselor", "Sessions" };
                        rows = m.Rows.Select(r => new[]
                        {
                        r.StudentName, r.GradeSection, r.DueDate.ToString("yyyy-MM-dd"),
                        r.DaysOverdue.ToString(), r.Status, r.CounselorName, r.SessionCount.ToString()
                    }).ToList();
                        break;
                    }

                case "drift":
                    {
                        var m = await _reports.DriftingStudentsAsync(filter, scope, Who);
                        name = "drifting-students";
                        headers = new[] { "Student", "Grade & section", "Last session", "Days since", "Sessions", "Last concern" };
                        rows = m.Rows.Select(r => new[]
                        {
                        r.StudentName, r.GradeSection, r.LastSessionDate.ToString("yyyy-MM-dd"),
                        r.DaysSince.ToString(), r.SessionCount.ToString(), r.LastConcern
                    }).ToList();
                        break;
                    }

                case "incidents":
                    {
                        var m = await _reports.IncidentsAsync(filter, Who);
                        name = "behavioral-incidents";
                        headers = new[] { "Student", "Location", "Records" };
                        rows = m.Rows.Select(r => new[]
                        {
                        r.StudentName, r.Place, r.Incidents.ToString()
                         }).ToList();
                        break;
                    }

                // The "when IsAdmin" guard matters: without it a counselor could
                // pull an admin report just by editing the query string.
                case "utilisation" when IsAdmin:
                    {
                        var m = await _reports.UtilisationAsync(filter, Who);
                        name = "appointment-utilisation";
                        headers = new[] { "Status", "Count", "Share %", "Note" };
                        rows = m.Rows.Select(r => new[]
                        {
                        r.Status, r.Count.ToString(), r.SharePercent.ToString(), r.Note
                    }).ToList();
                        break;
                    }

                case "workload" when IsAdmin:
                    {
                        var m = await _reports.WorkloadAsync(filter, Who);
                        name = "counselor-workload";
                        headers = new[] { "Counselor", "Position", "Sessions", "Open cases", "Overdue", "Share %" };
                        rows = m.Rows.Select(r => new[]
                        {
                        r.CounselorName, r.Position, r.Sessions.ToString(),
                        r.OpenCases.ToString(), r.Overdue.ToString(), r.SharePercent.ToString()
                    }).ToList();
                        break;
                    }

                case "audit" when IsAdmin:
                    {
                        var m = await _reports.AuditAsync(filter, Who);
                        name = "activity-log";
                        headers = new[] { "User", "Role", "Action", "Details", "When" };
                        rows = m.Rows.Select(r => new[]
                        {
                        r.Who, r.Role, r.Action, r.Details, r.When.ToString("yyyy-MM-dd HH:mm")
                    }).ToList();
                        break;
                    }

                default:
                    return NotFound();
            }

            var csv = BuildCsv(headers, rows);
            var stamp = DateTime.Now.ToString("yyyyMMdd");

            // UTF-8 BOM so Excel renders accented names correctly.
            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv))
                .ToArray();

            return File(bytes, "text/csv", $"gcams-{name}-{stamp}.csv");
        }

        private static string BuildCsv(string[] headers, List<string[]> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(Escape)));

            foreach (var row in rows)
                sb.AppendLine(string.Join(",", row.Select(Escape)));

            return sb.ToString();
        }

        /// <summary>
        /// Quotes anything containing a comma, quote or newline, and blocks
        /// formula injection — Excel executes a cell starting with = + - or @,
        /// so those get a leading apostrophe.
        /// </summary>
        private static string Escape(string? value)
        {
            var v = value ?? string.Empty;

            if (v.Length > 0 && "=+-@".Contains(v[0]))
                v = "'" + v;

            if (v.Contains(',') || v.Contains('"') || v.Contains('\n') || v.Contains('\r'))
                v = "\"" + v.Replace("\"", "\"\"") + "\"";

            return v;
        }
    }
}