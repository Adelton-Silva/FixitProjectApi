
namespace AuthenticationService.Models
{
    public class User
    {
        public required string Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
    }

    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
    }
}
