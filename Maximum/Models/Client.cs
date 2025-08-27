using System.ComponentModel.DataAnnotations;

namespace Maximum.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле обязательно")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некорректный телефон")]
        [StringLength(30, ErrorMessage = "Максимум 30 символов")]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный Email")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата рождения")]
        public DateOnly? BirthDate { get; set; }

        [StringLength(500, ErrorMessage = "Максимум 500 символов")]
        [Display(Name = "Заметки")]
        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}


