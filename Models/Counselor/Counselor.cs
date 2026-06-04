using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GCAMS.Models.Counselor
{
    public class Counselor
    {
        [Key]
        public int CounselorID { get; set; }

        [Required(ErrorMessage = "Employee number is required.")]
        [StringLength(20, ErrorMessage = "Employee number cannot exceed 20 characters.")]
        [Display(Name = "Employee Number")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Middle name cannot exceed 50 characters.")]
        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birth date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [StringLength(15, ErrorMessage = "Contact number cannot exceed 15 characters.")]
        [Phone(ErrorMessage = "Invalid contact number format.")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Educational attainment is required.")]
        [StringLength(100, ErrorMessage = "Educational attainment cannot exceed 100 characters.")]
        [Display(Name = "Educational Attainment")]
        public string EducationalAttainment { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "License number cannot exceed 50 characters.")]
        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        [Required(ErrorMessage = "Years of experience is required.")]
        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50.")]
        [Display(Name = "Years of Experience")]
        public int YearsOfExperience { get; set; }

        [Required(ErrorMessage = "Position is required.")]
        [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters.")]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date hired is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date Hired")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateHired { get; set; }

        [Required(ErrorMessage = "Employment status is required.")]
        [StringLength(20, ErrorMessage = "Employment status cannot exceed 20 characters.")]
        [Display(Name = "Employment Status")]
        public string EmploymentStatus { get; set; } = string.Empty;

        // Computed / NotMapped
        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName =>
            string.IsNullOrWhiteSpace(MiddleName)
                ? $"{LastName}, {FirstName}"
                : $"{LastName}, {FirstName} {MiddleName[0]}.";
    }
}
