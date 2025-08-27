using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maximum.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Клиент")]
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        [Required]
        [Display(Name = "Сотрудник")]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // Время храним в UTC
        [Required]
        [Display(Name = "Начало (UTC)")]
        public DateTime StartAtUtc { get; set; }

        [Required]
        [Display(Name = "Окончание (UTC)")]
        public DateTime EndAtUtc { get; set; }

        [StringLength(200, ErrorMessage = "Максимум 200 символов")]
        [Display(Name = "Тема")]
        public string? Title { get; set; }

        [StringLength(500, ErrorMessage = "Максимум 500 символов")]
        [Display(Name = "Заметки")]
        public string? Notes { get; set; }
    }
}


