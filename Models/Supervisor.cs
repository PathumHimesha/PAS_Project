using System.ComponentModel.DataAnnotations;

namespace PAS_Project.Models
{
    public class Supervisor
    {
        [Key]
        public int SupervisorId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PreferredResearchAreas { get; set; } // e.g., "AI, Web"

        public ICollection<Project> SupervisedProjects { get; set; }
    }
}