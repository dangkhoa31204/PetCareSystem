using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Dtos.Auth;
using PetCareSystem.API.Enums;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PetCareSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly PetCareSystemContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(PetCareSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Đăng kí tài khoản mới cho khách hàng (Customer)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == registerDto.Email);
            if (existingAccount != null)
            {
                return BadRequest("Email already exists.");
            }

            var account = new Account
            {
                Username = registerDto.Email, // Or generate a unique username
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = (int)AccountRole.Customer,
                CreatedAt = DateTime.UtcNow
            };

            var user = new User
            {
                FullName = registerDto.FullName,
                Phone = registerDto.Phone,
                CreatedAt = DateTime.UtcNow,
                Account = account
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Registration successful");
        }

        /// <summary>
        /// Đăng nhập và lấy token JWT
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Email == loginDto.Email);

            if (account == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, account.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            var token = CreateToken(account);

            // Get role name
            var roleEnum = (AccountRole)account.Role;
            var user = account.User;

            // Check for active membership
            bool isProMember = false;
            DateTime? proExpiredAt = null;

            if (user != null)
            {
                var activeMembership = _context.UserMemberships
                    .Where(m => m.UserId == user.UserId && 
                               m.Status == (int)UserMembershipStatus.Active &&
                               m.EndDate > DateTime.Now)
                    .FirstOrDefault();

                if (activeMembership != null)
                {
                    isProMember = true;
                    proExpiredAt = activeMembership.EndDate;
                }
            }

            var response = new LoginResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = 604800,
                User = new UserInfoDto
                {
                    UserId = user?.UserId ?? 0,
                    AccountId = account.AccountId,
                    Username = account.Username,
                    Email = account.Email,
                    FullName = user?.FullName ?? string.Empty,
                    Role = roleEnum.ToString(),
                    IsProMember = isProMember,
                    ProExpiredAt = proExpiredAt,
                    AvatarUrl = user?.AvatarUrl ?? string.Empty
                }
            };

            return Ok(response);
        }

        private string CreateToken(Account account)
        {
            var user = _context.Users.FirstOrDefault(u => u.AccountId == account.AccountId);

            // Get role name from enum
            var roleEnum = (AccountRole)account.Role;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Role, roleEnum.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
