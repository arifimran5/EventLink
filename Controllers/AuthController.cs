using EventLink.Data;
using EventLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventLink.Controllers
{
    using BCrypt.Net;

    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IConfiguration config;
        public AuthController(AppDbContext context, IConfiguration config)
        {
            this.context = context;
            this.config = config;
        }

        [HttpPost("/api/register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser user)
        {
            if (!ModelState.IsValid) return BadRequest();

            context.Add(new User()
            {
                Email = user.Email,
                Username = user.Username,
                Password = BCrypt.HashPassword(user.Password),
                Bio = user.Bio,
                Dob = user.Dob,
                CreatedAt = DateTime.UtcNow,
            });

            try
            {
                await context.SaveChangesAsync();
                return Created();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/api/login")]
        public IActionResult Login([FromBody] LoginUser user)
        {
            if (!ModelState.IsValid) return BadRequest();

            var foundUser = context.Users.FirstOrDefault(u => u.Email == user.Email);

            if (foundUser != null)
            {
                if (BCrypt.Verify(user.Password, foundUser.Password))
                {
                    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
                    var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                    Console.WriteLine(config["Jwt:Key"]);

                    var claims = new List<Claim>()
                    {
                        new(ClaimTypes.NameIdentifier, foundUser.Username),
                        new(ClaimTypes.Email, foundUser.Email),
                        new("UserId", foundUser.Id.ToString())
                    };

                    var token = new JwtSecurityToken(
                        config["Jwt:Issuer"],
                        config["Jwt:Audience"],
                        claims,
                        expires:DateTime.Now.AddHours(1),
                        signingCredentials: credentials
                    );
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                    return Ok(new { token = tokenString });
                }
            }

            return Unauthorized();
        }
    }
}
