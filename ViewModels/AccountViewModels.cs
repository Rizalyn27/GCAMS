using System.ComponentModel.DataAnnotations;

namespace GCAMS.ViewModels
{
    /// <summary>One row in the Accounts table.</summary>
    public class AccountRowViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        /// <summary>False means the user must still change their password.</summary>
        public bool PasswordChange { get; set; }

        /// <summary>The person's real name, resolved from Students/Counselors where one exists.</summary>
        public string? LinkedName { get; set; }

        /// <summary>True when this row is the signed-in admin's own account.</summary>
        public bool IsSelf { get; set; }

        /// <summary>Set when the row must not be deactivated, and says why.</summary>
        public string? LockReason { get; set; }
    }

    public class AccountIndexViewModel
    {
        public List<AccountRowViewModel> Accounts { get; set; } = new();

        public string? Search { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }

        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int AdminCount { get; set; }
    }

    /// <summary>
    /// Creating an admin. Deliberately NOT the Users entity: that model carries a
    /// [Required] ConfirmPassword and a username regex that rejects the email-style
    /// names the rest of the system uses, and binding it would expose Role and
    /// IsActive to whatever the browser posts.
    /// </summary>
    public class CreateAdminViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 30 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_.@-]+$",
            ErrorMessage = "Username can contain letters, numbers, and _ . @ - only.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Temporary password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$",
            ErrorMessage = "Password needs an uppercase letter, a lowercase letter, a number, and a special character (!@#$%^&*).")]
        [DataType(DataType.Password)]
        [Display(Name = "Temporary Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the password.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Temporary password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$",
            ErrorMessage = "Password needs an uppercase letter, a lowercase letter, a number, and a special character (!@#$%^&*).")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
    }
}