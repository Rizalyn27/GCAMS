using System.ComponentModel.DataAnnotations;
using GCAMS.Models.Counselor;

namespace GCAMS.Models.Announcements
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        public int? CounselorID { get; set; }
        public Counselor.Counselor? Counselor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}