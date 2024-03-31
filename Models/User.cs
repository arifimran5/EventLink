using System.ComponentModel.DataAnnotations;

namespace EventLink.Models
{
    public class User
    {
        public int Id { get; set; }

        [StringLength(20, ErrorMessage = "UserName length can't be more than 20")]
        public string Username { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Bio length can't be more than 100.")]
        public string? Bio { get; set; }
        public DateTime? Dob { get; set; }

        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ICollection<Event> HostedEvents { get; set; }
    }

    public class RegisterUser
    {
        public string Username { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public DateTime? Dob { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginUser
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CheckUser
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

    }

    public class LoginResponseUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public DateTime? Dob { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
