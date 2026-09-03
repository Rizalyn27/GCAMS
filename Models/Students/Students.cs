using GCAMS.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

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
        [FullName]
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
        [StudentBirthdate]
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

    public static class StudentRules
    {
        // Youngest / oldest a student may be. Used for both the server-side
        // check and the min/max on the date picker.
        public const int MinAge = 10;
        public const int MaxAge = 25;

        // Staff (counselors) have their own believable range.
        public const int MinStaffAge = 21;
        public const int MaxStaffAge = 70;

        // Every contact number in the system is a Philippine mobile number.
        // Stored canonically as 11 digits starting 09, but people are allowed to
        // type any of the usual formats and it gets converted on the way in.
        public const int MobileDigits = 11;

        /// <summary>Age on a given date, counted by birthday anniversary (not by dividing days).</summary>
        public static int AgeOn(DateTime birthday, DateTime asOf)
        {
            int age = asOf.Year - birthday.Year;
            if (birthday.Date > asOf.Date.AddYears(-age)) age--;
            return age < 0 ? 0 : age;
        }

        /// <summary>Strips spaces, dashes and parentheses so we only count digits.</summary>
        public static string DigitsOnly(string? value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : Regex.Replace(value, @"\D", "");

        /// <summary>
        /// Converts any of the formats people actually type into the single stored
        /// form, 09XXXXXXXXX:
        ///   +63 917 123 4567 / 639171234567 / 09171234567 / 9171234567 / 00639171234567
        /// Anything that is not recognisably a PH mobile number is returned unchanged,
        /// so the validation message shows the user what they actually typed.
        /// </summary>
        public static string NormalizeMobile(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value ?? string.Empty;

            var digits = DigitsOnly(value);

            // International dialling prefix, e.g. 0063...
            if (digits.StartsWith("00") && digits.Length > 2)
                digits = digits.Substring(2);

            // 639171234567 -> 09171234567
            if (digits.Length == 12 && digits.StartsWith("639"))
                return "0" + digits.Substring(2);

            // Already canonical.
            if (digits.Length == MobileDigits && digits.StartsWith("09"))
                return digits;

            // 9171234567 -> 09171234567
            if (digits.Length == 10 && digits.StartsWith("9"))
                return "0" + digits;

            return value;
        }

        /// <summary>True when the value is a PH mobile number in any accepted format.</summary>
        public static bool IsValidMobile(string? value)
        {
            var normalized = DigitsOnly(NormalizeMobile(value));
            return normalized.Length == MobileDigits && normalized.StartsWith("09");
        }

    }

    /// <summary>
    /// Rejects a single-word entry so "Imee" can't be saved where "Imee Reyes Santos" belongs.
    /// Requires at least two name parts of two or more letters each.
    /// </summary>
    public class FullNameAttribute : ValidationAttribute
    {
        private static readonly Regex AllowedCharacters =
            new(@"^[\p{L}\p{M}'.\- ]+$", RegexOptions.Compiled);

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var name = value as string;

            if (string.IsNullOrWhiteSpace(name))
                return ValidationResult.Success; // [Required] handles empty.

            name = name.Trim();

            if (!AllowedCharacters.IsMatch(name))
                return new ValidationResult("Name may only contain letters, spaces, hyphens, apostrophes and periods.");

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Where(p => p.Trim('.', '-', '\'').Length >= 2)
                            .ToList();

            if (parts.Count < 2)
                return new ValidationResult("Please enter the complete name (first name and surname), not just one name.");

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Keeps birthdates inside a believable range for a high school student.
    /// Blocks the "born in 2016" case the adviser flagged.
    /// </summary>
    public class StudentBirthdateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is null) return ValidationResult.Success;

            if (value is not DateTime birthday)
                return new ValidationResult("Invalid date of birth.");

            var today = DateTime.Today;

            if (birthday.Date > today)
                return new ValidationResult("Date of birth cannot be in the future.");

            int age = StudentRules.AgeOn(birthday, today);

            if (age < StudentRules.MinAge)
                return new ValidationResult($"Student must be at least {StudentRules.MinAge} years old.");

            if (age > StudentRules.MaxAge)
                return new ValidationResult($"Date of birth looks incorrect — that would make the student over {StudentRules.MaxAge} years old.");

            return ValidationResult.Success;
        }
    }
    /// <summary>
    /// Birthdate rule for staff records. A counselor cannot be 12 years old,
    /// and a typo of 1899 should not save either.
    /// </summary>
    public class StaffBirthdateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is null) return ValidationResult.Success;

            if (value is not DateTime birthday)
                return new ValidationResult("Invalid birth date.");

            if (birthday == default)
                return new ValidationResult("Birth date is required.");

            var today = DateTime.Today;

            if (birthday.Date > today)
                return new ValidationResult("Birth date cannot be in the future.");

            int age = StudentRules.AgeOn(birthday, today);

            if (age < StudentRules.MinStaffAge)
                return new ValidationResult($"Counselor must be at least {StudentRules.MinStaffAge} years old.");

            if (age > StudentRules.MaxStaffAge)
                return new ValidationResult($"Birth date looks incorrect - that would make the counselor over {StudentRules.MaxStaffAge} years old.");

            return ValidationResult.Success;
        }
    }


}