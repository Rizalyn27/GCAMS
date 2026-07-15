using GCAMS.Models.CaseNotes;
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

        // This is for the multi-value contact numbers
        public List<ContactEntry> StudentContacts { get; set; } = new();
        public List<ContactEntry> FatherContacts { get; set; } = new();
        public List<ContactEntry> MotherContacts { get; set; } = new();
        public List<ContactEntry> EmergencyContacts { get; set; } = new();

        // Case Notes for this student
        public List<CaseNotes> CaseNotes { get; set; } = new();
    }

    public class ContactEntry
    {
        public string Number { get; set; } = string.Empty;
        public string? Label { get; set; }
    }
}
