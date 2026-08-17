using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GCAMS.Models.Counselor
{
    public class Counselor
    {
        [Key]
        public int CounselorID { get; set; }

      
        // Personal Information
        [Required]
        [StringLength(50)]
        public string CounselorName { get; set; } = string.Empty;


        // Employee Information
        [Required]
        [StringLength(20)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;



      
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string EmailAddress { get; set; } = string.Empty;

        public List<CounselorContactNumber> ContactNumbers { get; set; } = new();

        [Required]
        [StringLength(20)]
        [Display(Name = "Marital Status")]
        public string MaritalStatus { get; set; } = string.Empty;

        [Required]
        [Range(0, 20)]
        [Display(Name = "No. of Children")]
        public int NumberOfChildren { get; set; }
        // Educational Background

        [StringLength(150)]
        [Display(Name = "College/University Attended")]
        public string? College { get; set; }

        [StringLength(150)]
        [Display(Name = "Course")]
        public string? CollegeCourse { get; set; }

        [StringLength(150)]
        [Display(Name = "Post Graduate Studies")]
        public string? PostGraduateStudies { get; set; }

        [StringLength(150)]
        [Display(Name = "Post Graduate Course")]
        public string? PostGraduateCourse { get; set; }


        // PRC License

        [StringLength(100)]
        [Display(Name = "PRC License")]
        public string? PRCLicense { get; set; }

        [StringLength(100)]
        [Display(Name = "PRC License 2")]
        public string? PRCLicense2 { get; set; }


        // Work Experience

        [StringLength(255)]
        [Display(Name = "Work Experience")]
        public string? WorkExperience { get; set; }

        [StringLength(255)]
        [Display(Name = "Work/School")]
        public string? WorkSchool { get; set; } = "Don Sergio Osmeña Sr. Memorial National High School";


        // Position / Designation

        [Required(ErrorMessage = "Position is required.")]
        [StringLength(100)]
        [Display(Name = "Position/Designation")]
        public string Position { get; set; } = string.Empty;


        // Employment Status

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string EmploymentStatus { get; set; }

        
    }
}