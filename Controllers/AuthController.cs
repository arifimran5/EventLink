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
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

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

            var doesUserExists = await context.Users.FirstOrDefaultAsync(u => u.Email == user.Email || u.Username == user.Username);

            if (doesUserExists != null)
            {
                return BadRequest("User with that Email or Username Already Exists");
            }

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
                return Ok(new AuthResponse
                {
                    Result = true,
                    Token = ""
                });
            }
            catch (DbUpdateException e)
            {

                if (e.InnerException?.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    return StatusCode(StatusCodes.Status409Conflict);
                }
                return BadRequest(e.Message);
            }
        }

        [HttpPost("/api/checkUser")]
        public async Task<IActionResult> CheckUserByUserName([FromBody] string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                var tempUser = await context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (tempUser != null)
                {
                    return Ok("user exists");
                }
                else
                {
                    return NotFound();
                }
            }

            return BadRequest();
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
                        expires: DateTime.Now.AddHours(1),
                        signingCredentials: credentials
                    );
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                    return Ok(new AuthResponse
                    {
                        Result = true,
                        Token = tokenString,
                        User = new LoginResponseUser()
                        {
                            Id = foundUser.Id,
                            Email = foundUser.Email,
                            Username = foundUser.Username,
                            Bio = foundUser.Bio,
                            Dob = foundUser.Dob,
                        }
                    });
                }
            }

            return Unauthorized();
        }
    }
}
