using GCAMS.Models.Appointment;

namespace GCAMS.ViewModels
{
    public class AppointmentCalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public List<CalendarDay> Days { get; set; } = new();
        public DateTime? SelectedDate { get; set; }
        public List<Appointments> SelectedDayAppointments { get; set; } = new();
    }
}
