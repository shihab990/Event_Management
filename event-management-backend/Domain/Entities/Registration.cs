using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Registration
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required int EventId { get; set; }
    }
}
