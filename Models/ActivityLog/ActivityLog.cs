using System.ComponentModel.DataAnnotations;

namespace GCAMS.Models.ActivityLog
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

        //Not Yet Done 
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
