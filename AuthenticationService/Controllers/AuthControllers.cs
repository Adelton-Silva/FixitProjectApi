using Microsoft.AspNetCore.Mvc;
using AuthenticationService.Models;
using AuthenticationService.Services;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Endpoint de Login (Autenticação)
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
        {
            var token = await _authService.Authenticate(request.Username, request.PasswordHash);
            if (token == null)
                return Unauthorized("Invalid credentials");

            return Ok(new { Token = token }); // Retorna o token de autenticação
        }

        // Endpoint de Registro
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.Register(request.Username, request.PasswordHash);
            if (result == "User already exists")
                return BadRequest(result); // Se o usuário já existir

            return Ok(new { Message = result }); // Retorna mensagem de sucesso
        }
    }
}
