using System.ComponentModel.DataAnnotations;

namespace Maximum.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле обязательно")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        [Display(Name = "Специализация")]
        public string? Specialty { get; set; }

        [Phone(ErrorMessage = "Некорректный телефон")]
        [StringLength(30, ErrorMessage = "Максимум 30 символов")]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный Email")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;
    }
}


