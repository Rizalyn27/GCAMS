using GCAMS.Models.Students;
using System.ComponentModel.DataAnnotations;

namespace GCAMS.Models.Counselor
{
    public class CounselorContactNumber
    {
        [Key] public int CounselorContactNumberID { get; set; }
        public int CounselorID { get; set; }
        public Counselor? Counselor { get; set; }

        [Phone]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;
        [StringLength(50)]
        public string? Label { get; set; }
    }
}
