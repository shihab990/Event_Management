using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class RegisterRequest
    {
        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
