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

    // ---------------------------------------------------------------
    // Counselor dashboard row types
    // ---------------------------------------------------------------

    /// <summary>A follow-up appointment whose date has passed without being resolved.</summary>
    public class FlaggedCaseItem
    {
        public int AppointmentId { get; set; }
        public int? StudentsID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime FollowUpDate { get; set; }
        public int DaysOverdue { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TodayAppointmentItem
    {
        public int AppointmentId { get; set; }
        public int? StudentsID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string AppointmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsUnassigned { get; set; }
    }

    /// <summary>A student who started counseling but has had no session for a while
    /// and has nothing scheduled — the "who am I forgetting?" list.</summary>
    public class DriftingStudentItem
    {
        public int StudentsID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string GradeSection { get; set; } = string.Empty;
        public DateTime LastSessionDate { get; set; }
        public int DaysSinceLastSession { get; set; }
        public int SessionCount { get; set; }
    }

    /// <summary>One bar in the "Top concerns" panel.</summary>
    public class ConcernCountItem
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>A recent anecdotal record surfaced on the dashboard.</summary>
    public class ObservationItem
    {
        public int AnecRecId { get; set; }
        public int StudentsID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime DateOfObserv { get; set; }
        public string Place { get; set; } = string.Empty;
        public string Behavior { get; set; } = string.Empty;
        /// <summary>True when this student has 2+ observations in the last 60 days.</summary>
        public bool IsRepeat { get; set; }
    }



    /// <summary>Lightweight student entry for the quick-action picker.</summary>
    public class StudentPickerOption
    {
        public int StudentsID { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class CounselorDashboardViewModel
    {
        public string CounselorName { get; set; } = string.Empty;

        // ---- Stat cards ----
        public int SessionsTodayCount { get; set; }
        /// <summary>Average sessions logged per working day over the last 30 days.</summary>
        public double AvgSessionsPerDay { get; set; }

        public int PendingRequestsCount { get; set; }
        /// <summary>Days the oldest pending request has been waiting. 0 when none.</summary>
        public int OldestPendingDays { get; set; }

        /// <summary>Days the most overdue follow-up has been waiting. 0 when none.</summary>
        public int MostOverdueDays { get; set; }

        public int CompletedAppointmentsCount { get; set; }

        // ---- Panels ----
        public List<TodayAppointmentItem> TodaysAppointments { get; set; } = new();
        public List<FlaggedCaseItem> FlaggedCases { get; set; } = new();
        public List<DriftingStudentItem> DriftingStudents { get; set; } = new();
        public List<ConcernCountItem> TopConcerns { get; set; } = new();
        public List<ObservationItem> RecentObservations { get; set; } = new();
        public List<StudentPickerOption> StudentPicker { get; set; } = new();

        // Mon..Sun case-note counts for the current week
        public int[] WeeklySessionLoad { get; set; } = new int[7];

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