namespace EventLink.Models
{
    public class AuthResponse
    {
        public bool Result { get; set; }
        public List<string>? Error { get; set; }
        public string Token { get; set; } = string.Empty;
        public LoginResponseUser? User { get; set; }
    }
}
