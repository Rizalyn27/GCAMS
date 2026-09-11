using System.ComponentModel.DataAnnotations;

namespace GCAMS.ViewModels
{
    /// <summary>
    /// Backs Users/MyProfile. This is a deliberately narrow view model: it carries
    /// ONLY the fields a signed-in user is allowed to change about themselves,
    /// plus some display-only strings for the header.
    ///
    /// Nothing here maps straight onto an entity. The controller copies each
    /// property across by hand, which is what stops a crafted POST from writing
    /// StuName, StuID, GradeLevel or Role — those properties simply do not exist
    /// on this type, so the model binder has nowhere to put them.
    /// </summary>
    public class MyProfileViewModel
    {
        // ------------------------------------------------------------------
        // Display only — never written back to the database.
        // ------------------------------------------------------------------
        public string Role { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool AccountIsActive { get; set; } = true;

        public string DisplayName { get; set; } = string.Empty;
        public string IdentifierLabel { get; set; } = "Username";
        public string IdentifierValue { get; set; } = string.Empty;

        /// <summary>Label/value pairs shown in the locked "Record details" panel.</summary>
        public List<ProfileFact> ReadOnlyFacts { get; set; } = new();

        // ------------------------------------------------------------------
        // Which blocks the view renders. Set by the controller from the role,
        // because the three roles genuinely have different fields available.
        // ------------------------------------------------------------------
        public bool HasEditableFields { get; set; }
        public bool ShowEmail { get; set; }
        public bool EmailIsEditable { get; set; }
        public bool ShowContactNumbers { get; set; }
        public bool ShowAddress { get; set; }
        public bool ShowLivingArrangement { get; set; }
        public bool ShowHealth { get; set; }
        public bool ShowEmergency { get; set; }

        /// <summary>Explains why a visible field is locked, when one is.</summary>
        public string? LockedFieldNote { get; set; }

        // ------------------------------------------------------------------
        // Editable — the whitelist.
        // ------------------------------------------------------------------
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(200)]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        /// <summary>Own mobile numbers. Blank entries are dropped on save.</summary>
        public List<string> ContactNumbers { get; set; } = new();

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Home Address")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "Living Arrangement")]
        public string? StayingWith { get; set; }

        // ---- Health (students only) ----
        [StringLength(10)]
        [Display(Name = "Blood Type")]
        public string? BloodType { get; set; }

        [StringLength(20)]
        [Display(Name = "Height (cm)")]
        public string? Height { get; set; }

        [StringLength(20)]
        [Display(Name = "Weight (kg)")]
        public string? Weight { get; set; }

        // ---- Emergency contact (students only) ----
        [StringLength(200)]
        [Display(Name = "Emergency Contact Person")]
        public string? EmergencyContactPerson { get; set; }

        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
        [Display(Name = "Age")]
        public int? EmergencyContactAge { get; set; }

        [StringLength(150)]
        [Display(Name = "Occupation")]
        public string? EmergencyContactOccupation { get; set; }

        [StringLength(300)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Address")]
        public string? EmergencyContactAddress { get; set; }

        public List<string> EmergencyContactNumbers { get; set; } = new();
    }

    public class ProfileFact
    {
        public ProfileFact() { }

        public ProfileFact(string label, string? value)
        {
            Label = label;
            Value = string.IsNullOrWhiteSpace(value) ? "—" : value;
        }

        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = "—";
    }
}