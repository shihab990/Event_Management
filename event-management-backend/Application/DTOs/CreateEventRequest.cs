using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateEventRequest : IValidatableObject
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "End time must be greater than start time.",
                    new[] { nameof(EndTime), nameof(StartTime) });
            }
        }
    }
}
