using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tickets.Data;
using Tickets.Models;
using Tickets.DTOs;

namespace Tickets.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            Console.WriteLine("EMP: " + login.NumeroEmpleado);
            Console.WriteLine("PASS: " + login.Password);

            var usuario = _context.Usuarios.FirstOrDefault(x =>
    x.NumeroEmpleado == login.NumeroEmpleado &&
    x.Password == login.Password &&
    x.Activo == true);

            if (usuario == null)
            {
                return Unauthorized("Credenciales incorrectas");
            }

            var claims = new[]
 {
    new Claim(ClaimTypes.Name, usuario.Nombre),

    new Claim(ClaimTypes.Role, usuario.Rol),

    new Claim("UsuarioId", usuario.Id.ToString())
};

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler()
        .WriteToken(token),

                rol = usuario.Rol,

                nombre = usuario.Nombre,

                usuarioId = usuario.Id
            });
        }
    }
}