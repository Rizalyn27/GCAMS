using System.ComponentModel.DataAnnotations;

namespace GCAMS.Models.Students
{
    public class HealthInformation
    {
        //Primary Key
        [Key]
        public int HealthInformationID { get; set; }

        //Foreign Key Student ID
        public int? StudentsID { get; set; }

        public Students? Student { get; set; } = null!;


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
        public string? SuicideRiskHistory { get; set; }

        //Victim of Abuse
        [Display(Name = "History of Abuse or Maltreatment")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? AbuseHistory { get; set; }

        //Involved with Illegal Drugs
        [Display(Name = "History of Illegal Drug Use")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? DrugHistory { get; set; }

        //Mentally Challenged Relative?
        [Display(Name = "Family History of Mental or Developmental Conditions")]
        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        public string? FamilyMentalHealthHistory { get; set; }

        //Visited Psychiatrist / Psychologist
        [Display(Name = "Previous Psychiatric or Psychological Consultation")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? PsychiatricConsultation { get; set; }

        //Additional Notes
        [Display(Name = "Additional Relevant Information / Remarks")]
        [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
        [DataType(DataType.MultilineText)]
        public string? AdditionalNotes { get; set; }
    }
}
