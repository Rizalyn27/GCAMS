using System.ComponentModel.DataAnnotations;

namespace GCAMS.Models.Students
{
    public class EmergencyContact
    {
        //Primary Key
        [Key]
        public int EmergencyContactID { get; set; }

        //Foreign Key Student ID
        public int? StudentsID { get; set; }

        public Students? Student { get; set; } = null!;

        // Emergency Contact Person
        [Display(Name = "Emergency Contact Person")]
        [StringLength(200)]
        public string? EmergencyContactPerson { get; set; }

        // Emergency Contact Age
        [Display(Name = "Emergency Contact Age")]
        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
        public int? EmergencyContactAge { get; set; }

        // Emergency Contact Occupation
        [Display(Name = "Emergency Contact Occupation")]
        [StringLength(150)]
        public string? EmergencyContactOccupation { get; set; }

        //Emergency Contact Number
        [Display(Name = "Emergency Contact Number")]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(20)]
        public string? EmergencyContactNumber { get; set; }

        //Emergency Contact Address
        [Display(Name = "Emergency Contact Address")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? EmergencyContactAddress { get; set; }

    }
}
