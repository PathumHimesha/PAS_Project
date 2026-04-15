using System.ComponentModel.DataAnnotations;

namespace PAS_Project.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public ICollection<Project> Projects { get; set; }
    }
}