using GCAMS.Models.Students;

namespace GCAMS.ViewModels
{
    public class StudentFormViewModel
    {
        public Students Student { get; set; } = new();
        public FamilyBackground Family { get; set; } = new();
        public EmergencyContact Emergency { get; set; } = new();
        public EducationalBackground Education { get; set; } = new();
        public HealthInformation Health { get; set; } = new();
    }
}
