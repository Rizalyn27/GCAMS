using System.ComponentModel.DataAnnotations;

namespace GCAMS.Models.ActivityLogs
{
    public enum ActivityAction
    {
        //Login
        SignIn,
        SignInFailed,

        //Student
        StudentAdded,
        StudentUpdated,
        StudentSetInactive,
        StudentSetActive,

        //AnnecRec
        AnnecRecCreated,
        AnnecRecUpdated,

        //Announcement
        AnnouncementCreated,

        //Appointment
        BookAppointment,
        CancelAppointment,
        RescheduleAppointment,

        //CaseNotes
        CaseNoteAdded,
        CaseNoteUpdated,

        //Counselor
        CounselorAdded,
        CounselorUpdated,

        //Users
        PasswordChanged,
        AccountCreated,
        AccountUpdated,
        AccountRemoved,

    }

    public class ActivityLog
    {

        [Key]

        public int Id { get; set; }
        public string Who { get; set; }
        public DateTime Date { get; set; }
        public string ActivityAction { get; set; }
        public string Details { get; set; }



    }
}
