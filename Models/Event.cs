using System.ComponentModel.DataAnnotations;

namespace EventLink.Models
{
    public class Event
    {
        public int Id { get; set; }

        [StringLength(60, ErrorMessage = "Event Name length can't be more than 60.")]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? HostId { get; set; } // why I made this FK nullable to remove cycle, I have NO CLUE
        public User Host { get; set; }
        public ICollection<EventAttendee> Attendees { get; set; }
    }

    public class CreateEvent
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile Image { get; set; }
        public DateTime Date { get; set; }
    }
}
