    using System.ComponentModel.DataAnnotations;

    namespace GCAMS.Models.Students
    {
        public class FamilyBackground
        {
            //Primary Key
            [Key]
            public int FamilyBackgroundID { get; set; }

            //Foreign Key Student ID
            public int? StudentsID { get; set; }

            public Students? Student { get; set; } = null!;


            // Father's Name
            [Display(Name = "Father's Name")]
            [StringLength(200)]
            public string? FatherName { get; set; }

            //Father's Age
            [Display(Name = "Father's Age")]
            [Range(18, 120, ErrorMessage = "Father's age must be between 18 and 120.")]
            public int? FatherAge { get; set; }

            //Father's Educational Attainment
            [Display(Name = "Father's Educational Attainment")]
            [StringLength(150)]
            public string? FatherEducationalAttainment { get; set; }

            //Father's Occupation
            [Display(Name = "Father's Occupation")]
            [StringLength(150)]
            public string? FatherOccupation { get; set; }

            //Mother's Name
            [Display(Name = "Mother's Name")]
            [StringLength(200)]
            public string? MotherName { get; set; }

            //Mother's Age
            [Display(Name = "Mother's Age")]
            [Range(18, 120, ErrorMessage = "Mother's age must be between 1 and 120.")]
            public int? MotherAge { get; set; }

            //Mother's Educational Attainment
            [Display(Name = "Mother's Educational Attainment")]
            [StringLength(150)]
            public string? MotherEducationalAttainment { get; set; }

            //Mother's Occupation
            [Display(Name = "Mother's Occupation")]
            [StringLength(150)]
            public string? MotherOccupation { get; set; }

            // Monthly Family Income
            [Display(Name = "Monthly Family Income")]
            [StringLength(50)]
            public string? MonthlyFamilyIncome { get; set; }

            // Parents' Relationship Status
            [Display(Name = "Parents' Relationship Status")]
            [StringLength(100)]
            public string? ParentsRelationshipStatus { get; set; }

            
            //Contact Number - This is for the multi-value contact numbers
            public ICollection<FamilyContactNumber> ContactNumbers { get; set; } = new List<FamilyContactNumber>();

    }
}
