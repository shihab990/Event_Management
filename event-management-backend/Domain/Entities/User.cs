using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string? JwtToken { get; set; }
    }
}
