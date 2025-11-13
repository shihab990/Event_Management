using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _users;
        private readonly IConfiguration _config;
        public AuthController(IUserService users, IConfiguration config)
        {
            _users = users;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _users.GetByUsernameAsync(req.Username);
            if (user == null) return Unauthorized();
            if (!PasswordHasher.Verify(req.Password, user.PasswordHash)) return Unauthorized();

            var jwt = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                },
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["DurationInMinutes"]!)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            await _users.SaveTokenAsync(user.Id, tokenStr);
            return Ok(new { token = tokenStr });
        }
    }
}
