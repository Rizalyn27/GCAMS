using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GCAMS.Models.Users;

namespace GCAMS.Models.Students
{
    public class Students
    {
        //Primary Key
        [Key]
        public int StudentsID { get; set; }

        //Foreign Key to Users
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public Users.Users? User { get; set; }

        // -------------------------
        // A. Personal Data
        // -------------------------

        //Student ID (The one given by the school, not the database ID)
        [Required(ErrorMessage = "Student ID is required.")]
        [Display(Name = "Student ID")]
        [StringLength(100, ErrorMessage = "Student ID cannot exceed 100 characters.")]
        public string StuID { get; set; } = string.Empty;

        //Full Name
        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string StuName { get; set; } = string.Empty;

        //Status (Active/Inactive)
        public bool IsActive { get; set; } = true;

        //Grade Level
        [Required(ErrorMessage = "Grade level is required.")]
        [Display(Name = "Grade Level")]
        [StringLength(50)]
        public string GradeLevel { get; set; } = string.Empty;

        //Section
        [Required(ErrorMessage = "Section is required.")]
        [StringLength(100)]
        public string Section { get; set; } = string.Empty;

        // School name
        [Required(ErrorMessage = "School is required.")]
        [StringLength(200)]
        public string? School { get; set; } = "Don Sergio Osmeña Sr. Memorial National High School";

        //Academic Year
        [Required(ErrorMessage = "Academic year is required.")]
        [StringLength(200)]
        public string? AcademicYear { get; set; }

        //Birthday
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }

        //Age
        [NotMapped]
        public int Age => Birthday.HasValue
        ? (int)((DateTime.Today - Birthday.Value).TotalDays / 365.25) : 0;

        //Birth Order
        [Display(Name = "Birth Order")]
        [StringLength(50)]
        public string? BirthOrder { get; set; }

        //Address
        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; } = string.Empty;

        //Contact Number
        public ICollection<StudentContactNumber> ContactNumbers { get; set; } = new List<StudentContactNumber>();


        //Email Address
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(200)]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        //Gender
        [StringLength(20)]
        public string? Gender { get; set; }

        //Nationality
        [StringLength(100)]
        public string? Nationality { get; set; }
        
        //Religion
        [StringLength(100)]
        public string? Religion { get; set; }

        // Staying With
        [Display(Name = "Living Arrangement")]
        [StringLength(100)]
        public string? StayingWith { get; set; }

        // B. Family Background
        public FamilyBackground? FamilyBackground { get; set; }

        // Emergency Contact    
        public EmergencyContact? EmergencyContact { get; set; }

        // C. Educational Background
        public EducationalBackground? EducationalBackground { get; set; }

        // D. Health Information
        public HealthInformation? HealthInformation { get; set; }

    }


}