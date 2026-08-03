using System.ComponentModel.DataAnnotations;

namespace GCAMS.Models.Notifs
{
    public enum NotificationType
    {
        AppointmentReminder,
        FollowUp,
        Announcement
    }
    public class Notifs
    {
       
            [Key]
            public int NotificationId { get; set; }

            // Matches User.Identity.Name (ClaimTypes.Name) exactly — no join needed.
            [Required]
            [MaxLength(100)]
            public string RecipientUsername { get; set; }

            [Required]
            public NotificationType Type { get; set; }

            [Required]
            [MaxLength(150)]
            public string Title { get; set; }

            [Required]
            [MaxLength(300)]
            public string Message { get; set; }

            // Optional: what this notification is "about" — lets the bell dropdown
            // link straight to the appointment/case note that triggered it.
            public string? RelatedEntityType { get; set; }   // e.g. "Appointment", "CaseNotes"
            public int? RelatedEntityId { get; set; }

            public bool IsRead { get; set; } = false;

            public DateTime CreatedAt { get; set; } = DateTime.Now;
        
    }
}
