using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationService.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthenticationService.Config;
using AuthenticationService.Services;
using RabbitMQ.Client;

namespace AuthenticationService.Services
{
    public interface IAuthService
    {
        Task<string> Authenticate(string username, string password);
        Task<string> Register(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _users;
        private readonly string _secretKey;
        private readonly RabbitMqPublisher _rabbitMqPublisher;

        public AuthService(IOptions<MongoDbSettings> settings, IOptions<JwtSettings> jwtSettings, RabbitMqPublisher rabbitMqPublisher)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
            _secretKey = jwtSettings.Value.SecretKey ?? throw new ArgumentNullException("SecretKey", "JWT secret key is missing.");
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        // Método de autenticação (login)
        public async Task<string> Authenticate(string username, string password)
        {
            var user = await _users.Find(u => u.Username == username && u.PasswordHash == password).FirstOrDefaultAsync();
            if (user == null) return null;

            // Envia mensagem para o RabbitMQ
            _rabbitMqPublisher.PublishMessage($"User {username} authenticated successfully");

            // Geração do token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("YourSecretKey");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Método de registro (criação de novo usuário)
        public async Task<string> Register(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return "Username and password are required";

            var userExists = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (userExists != null) return "User already exists";

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = passwordHash
            };

            await _users.InsertOneAsync(newUser);

            // Enviar uma mensagem para o RabbitMQ informando sobre o novo usuário
            _rabbitMqPublisher.PublishMessage($"New user registered: {newUser.Username}");

            return "User registered successfully";
        }
    }
}
