using GCAMS.Models.Appointment;
using GCAMS.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace GCAMS.Models.CaseNotes
{

    public class CaseNotes
    {

        public static readonly string[] ConcernCategories =
        {
            "Academic",
            "Behavioral",
            "Family",
            "Career",
            "Personal/Social",
            "Health"
        };

        //Primary Key
        [Key]
        public int CasenoteId { get; set; }

        //Foreign Key to Students table
        public int StudentsID { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(StudentsID))]
        public Students.Students Student { get; set; }


        // Who added this note — set automatically from the logged-in counselor, never from the form.

        public int? CounselorID { get; set; }
        [ValidateNever]
        [ForeignKey(nameof(CounselorID))]
        public Counselor.Counselor? Counselor { get; set; }


        //Name of Counselee
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        [Display(Name = "Name of Counselee")]
        public string FullName { get; set; } = string.Empty;

        //Session Number
        [Required(ErrorMessage = "Session No. is required")]
        [Display(Name = "Session No.")]
        public int SessionNo { get; set; }

        //Date
        [Required(ErrorMessage = "Session date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date")]
        public DateTime SessionDate { get; set; } = DateTime.Now;

        //Topics discussed during the session
        [Required]
        [StringLength(100)]
        [Display(Name = "Topics discussed during the session")]
        public string SessionTopics { get; set; }

        //Relevance of the session to the counseling plan
        [Required]
        [StringLength(100)]
        [Display(Name = "Relevance of the session to the counseling plan")]
        public string SessionRelevance { get; set; }

        //Means of achieving the counseling plan goals and objectives
        [Required]
        [StringLength(100)]
        [Display(Name = "Means of achieving the counseling plan goals and objectives")]
        public string GoalPlan { get; set; }

        //Interventions and techniques used during the session and their effectiveness
        [Required]
        [StringLength(100)]
        [Display(Name = "Interventions and techniques used during the session and their effectiveness")]
        public string Interventions { get; set; }

        //Counseling observations
        [Required]
        [StringLength(100)]
        [Display(Name = "Counseling observations")]
        public string Observations { get; set; }

        //Progress or setbacks
        [Required]
        [StringLength(100)]
        [Display(Name = "Progress or setbacks")]
        public string CounselProgess { get; set; }

        //Signs, symptoms, and any increase or decrease in the severity of behaviors as they relate to the main concern
        [Required]
        [StringLength(100)]
        [Display(Name = "Signs, symptoms, and any increase or decrease in the severity of behaviors as they relate to the main concern")]
        public string BehaviorStatus { get; set; }

        //FollowUpDate
        [FutureDate]
        [Display(Name = "Follow-up Date")]
        public DateTime? FollowUpDate { get; set; }


        //Homework assigned, results, and compliance (if any)
        [StringLength(100)]
        [Display(Name = "Homework assigned, results, and compliance (if any)")]
        public string? Homework { get; set; }

        //The counselee's current strengths and challenges
        [Required]
        [StringLength(100)]
        [Display(Name = "The counselee's current strengths and challenges")]
        public string StrengthsChallenges { get; set; }

        //Specific Goal
        [Required]
        [StringLength(100)]
        [Display(Name = "Relevance of the session to the counseling plan")]
        public string SpecificGoal { get; set; }

        [Required(ErrorMessage = "Please choose a concern category")]
        [StringLength(50)]
        [Display(Name = "Concern Category")]
        public string ConcernCategory { get; set; } = string.Empty;
    }
}

    