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
        [StringLength(11)]
        public string Number { get; set; } = string.Empty;
 
    }
}
