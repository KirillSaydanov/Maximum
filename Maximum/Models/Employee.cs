using System.ComponentModel.DataAnnotations;

namespace Maximum.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Specialty { get; set; }

        [Phone]
        [StringLength(30)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;
    }
}


