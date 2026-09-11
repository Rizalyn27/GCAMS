using System.ComponentModel.DataAnnotations;

namespace GCAMS.Models.Students
{
    public class EducationalBackground
    {
        //Primary Key
        [Key]
        public int EducationalBackgroundID { get; set; }

        //Foreign Key Student ID
        public int? StudentsID { get; set; }
        public Students? Student { get; set; } = null!;

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

    }
}
