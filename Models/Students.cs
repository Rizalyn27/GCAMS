using System.ComponentModel.DataAnnotations;

namespace GCAMS.Models
{
    public class Students
    {
        [Key]
        public int Id { get; set; }

        // -------------------------
        // A. Personal Data
        // -------------------------

        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Middle name is required.")]
        [Display(Name = "Middle Name")]
        [StringLength(100, ErrorMessage = "Middle name cannot exceed 100 characters.")]
        public string MName { get; set; } = string.Empty;

        // Computed full name (not stored in DB)
        [Display(Name = "Full Name")]
        public string FullName => $"{FName} {MName} {LName}".Trim();

        [Required(ErrorMessage = "Grade level is required.")]
        [Display(Name = "Grade Level")]
        [StringLength(50)]
        public string GradeLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section is required.")]
        [StringLength(100)]
        public string Section { get; set; } = string.Empty;

        // Defaults to the school name
        [Required(ErrorMessage = "School is required.")]
        [StringLength(200)]
        public string School { get; set; } = "";

        [Required(ErrorMessage = "Birthday is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(1, 100, ErrorMessage = "Age must be between 1 and 100.")]
        public int Age { get; set; }

        [Display(Name = "Birth Order")]
        [StringLength(50)]
        public string? BirthOrder { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Contact Number")]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(200)]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        [StringLength(100)]
        public string? Religion { get; set; }

        [Display(Name = "Staying With")]
        [StringLength(100)]
        public string? StayingWith { get; set; }

        // -------------------------
        // B. Family Background
        // -------------------------

        [Display(Name = "Father's Name")]
        [StringLength(200)]
        public string? FatherName { get; set; }

        [Display(Name = "Father's Age")]
        [Range(1, 120, ErrorMessage = "Father's age must be between 1 and 120.")]
        public int? FatherAge { get; set; }

        [Display(Name = "Father's Educational Attainment")]
        [StringLength(150)]
        public string? FatherEducationalAttainment { get; set; }

        [Display(Name = "Father's Occupation")]
        [StringLength(150)]
        public string? FatherOccupation { get; set; }

        [Display(Name = "Father's Contact Number")]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(20)]
        public string? FatherContactNumber { get; set; }

        [Display(Name = "Mother's Name")]
        [StringLength(200)]
        public string? MotherName { get; set; }

        [Display(Name = "Mother's Age")]
        [Range(1, 120, ErrorMessage = "Mother's age must be between 1 and 120.")]
        public int? MotherAge { get; set; }

        [Display(Name = "Mother's Educational Attainment")]
        [StringLength(150)]
        public string? MotherEducationalAttainment { get; set; }

        [Display(Name = "Mother's Occupation")]
        [StringLength(150)]
        public string? MotherOccupation { get; set; }

        [Display(Name = "Mother's Contact Number")]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(20)]
        public string? MotherContactNumber { get; set; }

        [Display(Name = "Monthly Family Income")]
        [StringLength(50)]
        public string? MonthlyFamilyIncome { get; set; }

        [Display(Name = "Parents' Relationship Status")]
        [StringLength(100)]
        public string? ParentsRelationshipStatus { get; set; }

        // -------------------------
        // Emergency Contact
        // -------------------------

        [Display(Name = "Emergency Contact Person")]
        [StringLength(200)]
        public string? EmergencyContactPerson { get; set; }

        [Display(Name = "Emergency Contact Age")]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        public int? EmergencyContactAge { get; set; }

        [Display(Name = "Emergency Contact Occupation")]
        [StringLength(150)]
        public string? EmergencyContactOccupation { get; set; }

        [Display(Name = "Emergency Contact Number")]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(20)]
        public string? EmergencyContactNumber { get; set; }

        [Display(Name = "Emergency Contact Address")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? EmergencyContactAddress { get; set; }

        // -------------------------
        // C. Educational Background
        // -------------------------

        [Display(Name = "Elementary School")]
        [StringLength(200)]
        public string? ElementarySchool { get; set; }

        [Display(Name = "Year Graduated (Elementary)")]
        [StringLength(20)]
        public string? ElementaryYear { get; set; }

        [Display(Name = "Honors (Elementary)")]
        [StringLength(150)]
        public string? ElementaryHonors { get; set; }

        [Display(Name = "Secondary School")]
        [StringLength(200)]
        public string? SecondarySchool { get; set; }

        [Display(Name = "Year Graduated (Secondary)")]
        [StringLength(20)]
        public string? SecondaryYear { get; set; }

        [Display(Name = "Honors (Secondary)")]
        [StringLength(150)]
        public string? SecondaryHonors { get; set; }

        // -------------------------
        // D. Health Information
        // -------------------------

        [StringLength(20)]
        public string? Height { get; set; }

        [StringLength(20)]
        public string? Weight { get; set; }

        [Display(Name = "Blood Type")]
        [StringLength(10)]
        public string? BloodType { get; set; }

        [Display(Name = "Ailments / Handicap")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? Ailments { get; set; }

        [Display(Name = "Under Medication")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? Medication { get; set; }

        // Sensitive mental health fields
        [Display(Name = "Suicidal Attempts or Thoughts")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? SuicidalAttempts { get; set; }

        [Display(Name = "Victim of Abuse")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? VictimOfAbuse { get; set; }

        [Display(Name = "Involved with Illegal Drugs")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? InvolvedWithDrugs { get; set; }

        [Display(Name = "Mentally Challenged Family Member / Relative")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? MentallyChallengedRelative { get; set; }

        [Display(Name = "Visited Psychiatrist / Psychologist")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? VisitedPsychiatrist { get; set; }

        [Display(Name = "Reason for Visiting Psychiatrist / Psychologist")]
        [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
        [DataType(DataType.MultilineText)]
        public string? VisitedPsychiatristReason { get; set; }
    }
}