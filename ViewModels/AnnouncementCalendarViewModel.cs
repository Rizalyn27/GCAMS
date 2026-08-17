using GCAMS.Models.Announcements;

namespace GCAMS.ViewModels
{
    public class AnnouncementCalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public List<CalendarDay> Days { get; set; } = new();
        public DateTime? SelectedDate { get; set; }
        public List<Announcement> SelectedDayAnnouncements { get; set; } = new();
    }

    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public int Count { get; set; }
    }
}
