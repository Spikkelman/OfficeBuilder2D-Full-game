using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
         _context = context;
         _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username already exists.");

        if (!IsValidPassword(request.Password))
            return BadRequest("Password must be at least 10 characters long and contain at least one lowercase letter, one uppercase letter, one digit, and one non-alphanumeric character.");

        CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User {
            Username = request.Username,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
         var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
         if (user == null)
             return BadRequest("User not found.");

         if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
             return BadRequest("Incorrect password.");

         string token = CreateToken(user);
         return Ok(new { token });
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
         using (var hmac = new HMACSHA512())
         {
             passwordSalt = hmac.Key;
             passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
         }
    }

    private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
         using (var hmac = new HMACSHA512(storedSalt))
         {
             var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
             return computedHash.SequenceEqual(storedHash);
         }
    }

    // Validate password complexity.
    private bool IsValidPassword(string password)
    {
         if (password.Length < 10) return false;
         bool hasLower = false, hasUpper = false, hasDigit = false, hasNonAlphanumeric = false;
         foreach (char c in password)
         {
             if (char.IsLower(c)) hasLower = true;
             else if (char.IsUpper(c)) hasUpper = true;
             else if (char.IsDigit(c)) hasDigit = true;
             else if (!char.IsLetterOrDigit(c)) hasNonAlphanumeric = true;
         }
         return hasLower && hasUpper && hasDigit && hasNonAlphanumeric;
    }

    private string CreateToken(User user)
    {
         List<Claim> claims = new List<Claim> {
             new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
             new Claim(ClaimTypes.Name, user.Username)
         };

         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

         var token = new JwtSecurityToken(
             claims: claims,
             expires: DateTime.Now.AddDays(1),
             signingCredentials: creds
         );

         return new JwtSecurityTokenHandler().WriteToken(token);
    }
}