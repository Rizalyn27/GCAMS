using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GCAMS.Models.Students;
using GCAMS.Models.Counselor;

namespace GCAMS.Models.Appointment
{
    public class Appointments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [HiddenInput(DisplayValue = false)]
        public int AppointmentID { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email Address")]
        public string? Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date & Time")]
        [FutureDate(ErrorMessage = "Appointment date and time must be in the future.")]
        public DateTime AppointmentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Appointment type is required")]
        [StringLength(100)]
        [Display(Name = "Appointment Type")]
        public string AppointmentType { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime? UpdatedAt { get; set; }

        // Link to Student
        public int? StudentsID { get; set; }
        [ForeignKey(nameof(StudentsID))]
        public Students.Students? Student { get; set; }

        // Which counselor is handling this — claimed automatically by whoever first acts on it
        public int? CounselorID { get; set; }
        [ForeignKey(nameof(CounselorID))]
        public Counselor.Counselor? Counselor { get; set; }
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is DateTime date)
            {
                if (date <= DateTime.Now)
                    return new ValidationResult("Appointment date and time must be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}