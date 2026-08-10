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

        // Leave blank to target everyone
        [StringLength(50)]
        public string? GradeLevel { get; set; }

        [StringLength(100)]
        public string? Section { get; set; }

        public int? CounselorID { get; set; }
        public Counselor.Counselor? Counselor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}