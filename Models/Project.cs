using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAS_Project.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Abstract { get; set; }

        [Required]
        public string TechnicalStack { get; set; }

        [Required]
        public string ResearchArea { get; set; }

        [Required]
        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public int? SupervisorId { get; set; }
        [ForeignKey("SupervisorId")]
        public Supervisor Supervisor { get; set; }
    }
}