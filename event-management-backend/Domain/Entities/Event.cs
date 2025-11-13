using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Location { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }

        [JsonIgnore]
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
