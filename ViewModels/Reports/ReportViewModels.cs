using System.ComponentModel.DataAnnotations;

namespace GCAMS.ViewModels.Reports
{
    public class ReportFilter
    {
        [Display(Name = "Academic year")]
        public string? AcademicYear { get; set; }      // "2026-2027"

        [Display(Name = "Month")]
        public int? Month { get; set; }                // null = whole AY

        [Display(Name = "Grade level")]
        public string? GradeLevel { get; set; }

        [Display(Name = "Counselor")]   
        public int? CounselorId { get; set; }

        [Display(Name = "Overdue by")]
        public int? MinDaysOverdue { get; set; }

        [Display(Name = "Gap threshold")]
        public int DriftThresholdDays { get; set; } = 30;

        [Display(Name = "From")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "User")]
        public string? User { get; set; }

        [Display(Name = "Action")]
        public string? Action { get; set; }

        // Populated by the controller for the dropdowns
        public List<string> AcademicYearOptions { get; set; } = new();
        public List<string> GradeLevelOptions { get; set; } = new();
        public List<(int Id, string Name)> CounselorOptions { get; set; } = new();

        /// <summary>True when the signed-in user may pick a counselor other
        /// than themselves (admins) — drives whether that filter renders.</summary>
        public bool CanChooseCounselor { get; set; }
    }

    /// <summary>One KPI in the strip above each table.</summary>
    public class ReportKpi
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Note { get; set; }

        /// <summary>Renders amber. Set when a threshold has been crossed —
        /// this is what turns a number into something worth acting on.</summary>
        public bool IsWarning { get; set; }
    }

    public abstract class ReportBase
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;

        /// <summary>What this report lets the reader decide. Shown on screen
        /// and printed into the export.</summary>
        public string DecisionLine { get; set; } = string.Empty;

        public string SourceNote { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string GeneratedBy { get; set; } = string.Empty;

        public ReportFilter Filter { get; set; } = new();
        public List<ReportKpi> Kpis { get; set; } = new();
    }

    // =================================================================
    // 1. Counseling Summary
    // =================================================================
    public class ConcernRow
    {
        public string Category { get; set; } = string.Empty;
        public int Sessions { get; set; }
        public int Students { get; set; }
        public string TopGradeLevel { get; set; } = string.Empty;
        public int TopGradeCount { get; set; }
        public double SharePercent { get; set; }
        public bool IsLead { get; set; }
    }

    public class CounselingSummaryReport : ReportBase
    {
        public List<ConcernRow> Rows { get; set; } = new();
        public int TotalSessions { get; set; }
    }

    // =================================================================
    // 2. Follow-up Compliance
    // =================================================================
    public class FollowUpRow
    {
        public int AppointmentId { get; set; }
        public int? StudentsID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string GradeSection { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CounselorName { get; set; } = string.Empty;
        public int SessionCount { get; set; }
    }

    public class FollowUpComplianceReport : ReportBase
    {
        public List<FollowUpRow> Rows { get; set; } = new();
        public int TotalScheduled { get; set; }
        public int TotalOverdue { get; set; }
        public double CompliancePercent { get; set; }
    }

    // =================================================================
    // 3. Drifting Students
    // =================================================================
    public class DriftRow
    {
        public int StudentsID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string GradeSection { get; set; } = string.Empty;
        public DateTime LastSessionDate { get; set; }
        public int DaysSince { get; set; }
        public int SessionCount { get; set; }
        public string LastConcern { get; set; } = string.Empty;
    }

    public class DriftingStudentsReport : ReportBase
    {
        public List<DriftRow> Rows { get; set; } = new();
        public int StudentsWithHistory { get; set; }
    }

    // =================================================================
    // 4. Behavioral Incidents
    // =================================================================
    public class IncidentRow
    {
        public int StudentsID { get; set; }
        public string StudentName { get; set; } = "";
        public string Place { get; set; } = "";
        public int Incidents { get; set; }
        public bool IsLead { get; set; }
    }

    public class IncidentReport : ReportBase
    {
        public List<IncidentRow> Rows { get; set; } = new();
        public int TotalIncidents { get; set; }
    }

    // =================================================================
    // 5. Appointment Utilisation (admin)
    // =================================================================
    public class UtilisationRow
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public double SharePercent { get; set; }
        public bool IsLead { get; set; }
        public string Note { get; set; } = string.Empty;
        public bool NoteIsWarning { get; set; }
    }

    public class UtilisationReport : ReportBase
    {
        public List<UtilisationRow> Rows { get; set; } = new();
        public int TotalRequests { get; set; }
        public double? AvgResponseHours { get; set; }
        public int PendingOver48h { get; set; }
        public string PeakDay { get; set; } = string.Empty;
    }

    // =================================================================
    // 6. Counselor Workload (admin)
    // =================================================================
    public class WorkloadRow
    {
        public int? CounselorId { get; set; }
        public string CounselorName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int Sessions { get; set; }
        public int OpenCases { get; set; }
        public int Overdue { get; set; }
        public double SharePercent { get; set; }
        public bool IsLead { get; set; }
    }

    public class WorkloadReport : ReportBase
    {
        public List<WorkloadRow> Rows { get; set; } = new();
        public int UnassignedAppointments { get; set; }
        public int ActiveCounselors { get; set; }
        public int StudentsPerCounselor { get; set; }
    }

    // =================================================================
    // 7. Activity Log (admin)
    // =================================================================
    public class AuditRow
    {
        public string Who { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime When { get; set; }
    }

    public class AuditReport : ReportBase
    {
        public List<AuditRow> Rows { get; set; } = new();
        public int TotalEvents { get; set; }
        public List<string> UserOptions { get; set; } = new();
        public List<string> ActionOptions { get; set; } = new();
    }

    // =================================================================
    // The tabbed Reports page — holds every report at once.
    // Admin-only reports stay null for counselors and the view skips
    // those tabs.
    // =================================================================
    public class ReportsPageViewModel
    {
        public ReportFilter Filter { get; set; } = new();
        public bool IsAdmin { get; set; }

        // Shown to everyone
        public CounselingSummaryReport Summary { get; set; } = new();
        public FollowUpComplianceReport FollowUp { get; set; } = new();
        public DriftingStudentsReport Drift { get; set; } = new();
        public IncidentReport Incidents { get; set; } = new();

        // Admin only — null for counselors
        public UtilisationReport? Utilisation { get; set; }
        public WorkloadReport? Workload { get; set; }
        public AuditReport? Audit { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string PeriodLabel { get; set; } = string.Empty;

        /// <summary>Which tab opens first. Preserved across Apply so the
        /// filter form doesn't bounce the user back to tab one.</summary>
        public string ActiveTab { get; set; } = "summary";
    }
}
