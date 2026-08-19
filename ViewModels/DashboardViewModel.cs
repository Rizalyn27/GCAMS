namespace GCAMS.ViewModels
{
    public class ActivityItem
    {
        public string Icon { get; set; } = "info";
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalActiveStudents { get; set; }
        public int ActiveCounselors { get; set; }
        public int AppointmentsThisWeek { get; set; }
        public int OpenCases { get; set; }

        // Mon..Sun counts for the current week, used for the bar chart
        public int[] WeeklyAppointmentCounts { get; set; } = new int[7];

        public int StudentUserCount { get; set; }
        public int CounselorUserCount { get; set; }
        public int AdminUserCount { get; set; }

        public List<ActivityItem> RecentActivity { get; set; } = new();

        public List<(string Username, string Role)> PendingSetupAccounts { get; set; } = new();
    }

    public class FlaggedCaseItem
    {
        public string StudentName { get; set; } = string.Empty;
        public DateTime FollowUpDate { get; set; }
        public string BehaviorStatus { get; set; } = string.Empty;
    }

    public class TodayAppointmentItem
    {
        public int AppointmentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string AppointmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CounselorDashboardViewModel
    {
        public string CounselorName { get; set; } = string.Empty;

        public int SessionsTodayCount { get; set; }
        public int OpenCasesCount { get; set; }
        public int PendingRequestsCount { get; set; }

        public List<TodayAppointmentItem> TodaysAppointments { get; set; } = new();

        // Mon..Sun case-note counts for the current week
        public int[] WeeklySessionLoad { get; set; } = new int[7];

        public List<FlaggedCaseItem> FlaggedCases { get; set; } = new();

        // ---- Session history filter (academic year + month) ----
        public string SelectedAcademicYear { get; set; } = string.Empty;
        public int? SelectedMonth { get; set; } // null = whole academic year
        public List<string> AcademicYearOptions { get; set; } = new();
        public string PeriodLabel { get; set; } = string.Empty;
        public int SessionsInPeriod { get; set; }
        public List<string> PeriodChartLabels { get; set; } = new();
        public List<int> PeriodChartCounts { get; set; } = new();
    }
}