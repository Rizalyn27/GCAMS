namespace GCAMS.ViewModels
{
    public class StudentBulkRow
    {
        public string StuID { get; set; } = "";
        public string StuName { get; set; } = "";
        public string GradeLevel { get; set; } = "";
        public string Section { get; set; } = "";
        public string? School { get; set; }
        public DateTime? Birthday { get; set; }
        public string? AcademicYear { get; set; }
        public string? BirthOrder { get; set; }
        public string Address { get; set; } = "";
        public string? StudentContact { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? Religion { get; set; }
        public string? StayingWith { get; set; }
        public string? FatherName { get; set; }
        public int? FatherAge { get; set; }
        public string? FatherEducationalAttainment { get; set; }
        public string? FatherOccupation { get; set; }
        public string? FatherContact { get; set; }
        public string? MotherName { get; set; }
        public int? MotherAge { get; set; }
        public string? MotherEducationalAttainment { get; set; }
        public string? MotherOccupation { get; set; }
        public string? MotherContact { get; set; }
        public string? MonthlyFamilyIncome { get; set; }
        public string? ParentsRelationshipStatus { get; set; }
        public string? EmergencyContactPerson { get; set; }
        public int? EmergencyContactAge { get; set; }
        public string? EmergencyContactOccupation { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public string? EmergencyContactAddress { get; set; }
        public string? ElementarySchool { get; set; }
        public string? ElementaryYear { get; set; }
        public string? ElementaryHonors { get; set; }
        public string? SecondarySchool { get; set; }
        public string? SecondaryYear { get; set; }
        public string? SecondaryHonors { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
    }

    public class BulkUpdatePreviewRow
    {
        public string StuID { get; set; } = "";
        public string StuName { get; set; } = "";
        public bool Found { get; set; }
        public List<string> ChangedFields { get; set; } = new();
    }

    public class BulkUpdatePreviewViewModel
    {
        public List<BulkUpdatePreviewRow> Rows { get; set; } = new();
        public int FoundCount => Rows.Count(r => r.Found);
        public int NotFoundCount => Rows.Count(r => !r.Found);
        public string PayloadJson { get; set; } = "";
    }
}