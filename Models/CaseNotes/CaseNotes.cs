using GCAMS.Models.Appointment;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GCAMS.Models.CaseNotes
{
    public class CaseNotes
    {

        //Primary Key

        [Key]
        public int CasenoteId { get; set; }

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

        //Homework assigned, results, and compliance (if any)
        [StringLength(100)]
        [Display(Name = "Homework assigned, results, and compliance (if any)")]
        public string? Homework { get; set; }

        //The counselee's current strengths and challenges
        [Required]
        [StringLength(100)]
        [Display(Name = "The counselee's current strengths and challenges")]
        public string StrengthsChallenges { get; set; }

        //Spefific Goal
        [Required]
        [StringLength(100)]
        [Display(Name = "Relevance of the session to the counseling plan")]
        public string SpecificGoal { get; set; }


    }
}
