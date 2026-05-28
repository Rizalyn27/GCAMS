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

        //Student ID
        [Required(ErrorMessage = "Student ID is required.")]
        [Display(Name = "Student ID")]
        [StringLength(100, ErrorMessage = "Student ID cannot exceed 100 characters.")]
        public string StudentId { get; set; } = string.Empty;

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
        public string School { get; set; } = string.Empty;

        //Birthday
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }

        //Age
        [Required(ErrorMessage = "Age is required.")]
        [Range(1, 100, ErrorMessage = "Age must be between 1 and 100.")]
        public int Age { get; set; }

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
        [Display(Name = "Contact Number")]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(20)]
        public string? ContactNumber { get; set; }

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

        // -------------------------
        // B. Family Background
        // -------------------------

        // Father's Name
        [Display(Name = "Father's Name")]
        [StringLength(200)]
        public string? FatherName { get; set; }

        //Father's Age
        [Display(Name = "Father's Age")]
        [Range(1, 120, ErrorMessage = "Father's age must be between 1 and 120.")]
        public int? FatherAge { get; set; }

        //Father's Educational Attainment
        [Display(Name = "Father's Educational Attainment")]
        [StringLength(150)]
        public string? FatherEducationalAttainment { get; set; }

        //Father's Occupation
        [Display(Name = "Father's Occupation")]
        [StringLength(150)]
        public string? FatherOccupation { get; set; }

        //Father's Contact Number
        [Display(Name = "Father's Contact Number")]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(20)]
        public string? FatherContactNumber { get; set; }

        //Mother's Name
        [Display(Name = "Mother's Name")]
        [StringLength(200)]
        public string? MotherName { get; set; }

        //Mother's Age
        [Display(Name = "Mother's Age")]
        [Range(1, 120, ErrorMessage = "Mother's age must be between 1 and 120.")]
        public int? MotherAge { get; set; }

        //Mother's Educational Attainment
        [Display(Name = "Mother's Educational Attainment")]
        [StringLength(150)]
        public string? MotherEducationalAttainment { get; set; }

        //Mother's Occupation
        [Display(Name = "Mother's Occupation")]
        [StringLength(150)]
        public string? MotherOccupation { get; set; }
        
        //Mother's Contact Number
        [Display(Name = "Mother's Contact Number")]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(20)]
        public string? MotherContactNumber { get; set; }

        // Monthly Family Income
        [Display(Name = "Monthly Family Income")]
        [StringLength(50)]
        public string? MonthlyFamilyIncome { get; set; }

        // Parents' Relationship Status
        [Display(Name = "Parents' Relationship Status")]
        [StringLength(100)]
        public string? ParentsRelationshipStatus { get; set; }

        // -------------------------
        // Emergency Contact
        // -------------------------

        // Emergency Contact Person
        [Display(Name = "Emergency Contact Person")]
        [StringLength(200)]
        public string? EmergencyContactPerson { get; set; }

        // Emergency Contact Age
        [Display(Name = "Emergency Contact Age")]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
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

        // -------------------------
        // C. Educational Background
        // -------------------------

        //Elementary School
        [Display(Name = "Elementary School")]
        [StringLength(200)]
        public string? ElementarySchool { get; set; }

        //Year (Elementary)
        [Display(Name = "Year (Elementary)")]
        [StringLength(20)]
        public string? ElementaryYear { get; set; }

        //Honors (Elementary)
        [Display(Name = "Honors (Elementary)")]
        [StringLength(150)]
        public string? ElementaryHonors { get; set; }

        //Secondary School
        [Display(Name = "Secondary School")]
        [StringLength(200)]
        public string? SecondarySchool { get; set; }

        //Year (Secondary)
        [Display(Name = "Year (Secondary)")]
        [StringLength(20)]
        public string? SecondaryYear { get; set; }

        //Honors (Secondary)
        [Display(Name = "Honors (Secondary)")]
        [StringLength(150)]
        public string? SecondaryHonors { get; set; }

        // -------------------------
        // D. Health Information
        // -------------------------

        //Height
        [StringLength(20)]
        public string? Height { get; set; }

        //Weight
        [StringLength(20)]
        public string? Weight { get; set; }

        //Blood Type
        [Display(Name = "Blood Type")]
        [StringLength(10)]
        public string? BloodType { get; set; }

        //Ailments / Handicap
        [Display(Name = "History of Medical Conditions or Physical Disabilities")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? Ailments { get; set; }

        //Medication
        [Display(Name = "Current Medications")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? Medication { get; set; }

        // Sensitive mental health fields

        //Suicidal Attempts or Thoughts
        [Display(Name = "History of Suicidal Thoughts or Attempts")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? SuicidalAttempts { get; set; }

        //Victim of Abuse
        [Display(Name = "History of Abuse or Maltreatment")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? VictimOfAbuse { get; set; }

        //Involved with Illegal Drugs
        [Display(Name = "History of Illegal Drug Use")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? InvolvedWithDrugs { get; set; }

        //Mentally Challenged Relative?
        [Display(Name = "Family History of Mental or Developmental Conditions")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? MentallyChallengedRelative { get; set; }

        //Visited Psychiatrist / Psychologist
        [Display(Name = "Previous Psychiatric or Psychological Consultation")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? VisitedPsychiatrist { get; set; }

        //Additional Notes
        [Display(Name = "Additional Relevant Information / Remarks")]
        [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
        [DataType(DataType.MultilineText)]
        public string? AdditionalNotes { get; set; }
    }
}