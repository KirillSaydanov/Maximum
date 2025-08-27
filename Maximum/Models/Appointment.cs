using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maximum.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // Время храним в UTC
        [Required]
        public DateTime StartAtUtc { get; set; }

        [Required]
        public DateTime EndAtUtc { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}


