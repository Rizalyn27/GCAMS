// Models/Students/ContactNumbers.cs
using System.ComponentModel.DataAnnotations;

namespace GCAMS.Models.Students
{
    public class StudentContactNumber
    {
        [Key] public int StudentContactNumberID { get; set; }
        public int StudentsID { get; set; }
        public Students? Student { get; set; }

        [Phone]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;
        [StringLength(50)]
        public string? Label { get; set; }
    }

    public class FamilyContactNumber
    {
        [Key] public int FamilyContactNumberID { get; set; }
        public int FamilyBackgroundID { get; set; }
        public FamilyBackground? FamilyBackground { get; set; }

        [Phone]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;
        [StringLength(50)]
        public string? Label { get; set; }
    }

    public class EmergencyContactNumber
    {
        [Key] public int EmergencyContactNumberID { get; set; }
        public int EmergencyContactID { get; set; }
        public EmergencyContact? EmergencyContact { get; set; }

        [Phone]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;
        [StringLength(50)]
        public string? Label { get; set; }
    }
}