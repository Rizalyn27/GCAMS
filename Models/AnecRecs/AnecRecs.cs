using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GCAMS.Models.AnecRecs
{
    public class AnecRecs
    {
        [Key]
        public int AnecRecsId { get; set; }

        public int StudentsID { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(StudentsID))]
        public Students.Students Student { get; set; }

        [Required]
        public string? StuName { get; set; }

        [Required]
        public DateTime DateOfObserv { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(100)]
        public string ObservedBy { get; set; }

        [Required]
        public int AnecRecNo { get; set; }

        [Required]
        [MaxLength(100)]
        public string Place { get; set; }

        [Required]
        [MaxLength(100)]
        public string PeopleInvolved { get; set; }

        [Required]
        [MaxLength(100)]
        public string SceneMood { get; set; }

        [Required]
        [MaxLength(100)]
        public string StudentBehavior { get; set; }

        [Required]
        [MaxLength(100)]
        public string ObserverRecs { get; set; }
    }
}