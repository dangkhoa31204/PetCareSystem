using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Dtos.Admin;
using PetCareSystem.API.Enums;
using PetCareSystem.API.Models;

namespace PetCareSystem.API.Controllers
{
    [Route("api/admin/accounts")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAccountController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public AdminAccountController(PetCareSystemContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<AccountResponseDto>>> GetAccounts([FromQuery] AccountRole? role, [FromQuery] AccountStatus? status)
        {
            var query = _context.Accounts
                .Include(a => a.User)
                .AsQueryable();

            if (role.HasValue)
            {
                query = query.Where(a => a.Role == (int)role.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == (int)status.Value);
            }

            var accounts = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var result = accounts.Select(MapToAccountResponse).ToList();
            return Ok(result);
        }

        [HttpGet("{accountId:long}")]
        public async Task<ActionResult<AccountResponseDto>> GetAccount(long accountId)
        {
            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
            {
                return NotFound("Account not found");
            }

            return Ok(MapToAccountResponse(account));
        }

        [HttpPost]
        public async Task<ActionResult<AccountResponseDto>> CreateAccount(CreateAccountDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == dto.Email);
            if (existing != null)
            {
                return BadRequest("Email already exists");
            }

            var account = new Account
            {
                Username = dto.Email,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = (int)dto.Role,
                Status = (int)(dto.Status ?? AccountStatus.Active),
                CreatedAt = DateTime.UtcNow
            };

            var user = new User
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                Specialization = dto.Specialization,
                CreatedAt = DateTime.UtcNow,
                Account = account
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccount), new { accountId = account.AccountId }, MapToAccountResponse(account));
        }

        [HttpPut("{accountId:long}")]
        public async Task<IActionResult> UpdateAccount(long accountId, UpdateAccountDto dto)
        {
            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
            {
                return NotFound("Account not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != account.Email)
            {
                var existing = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == dto.Email && a.AccountId != accountId);
                if (existing != null)
                {
                    return BadRequest("Email already exists");
                }

                account.Email = dto.Email;
                account.Username = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (dto.Role.HasValue)
            {
                account.Role = (int)dto.Role.Value;
            }

            if (dto.Status.HasValue)
            {
                account.Status = (int)dto.Status.Value;
            }

            if (account.User != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.FullName))
                {
                    account.User.FullName = dto.FullName;
                }

                if (dto.Phone != null)
                {
                    account.User.Phone = dto.Phone;
                }

                if (dto.Specialization != null)
                {
                    account.User.Specialization = dto.Specialization;
                }

                account.User.UpdatedAt = DateTime.UtcNow;
            }

            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Account updated successfully");
        }

        [HttpDelete("{accountId:long}")]
        public async Task<IActionResult> DeleteAccount(long accountId)
        {
            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
            {
                return NotFound("Account not found");
            }

            if (account.User != null)
            {
                _context.Users.Remove(account.User);
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return Ok("Account deleted successfully");
        }

        private static AccountResponseDto MapToAccountResponse(Account account)
        {
            return new AccountResponseDto
            {
                AccountId = account.AccountId,
                UserId = account.User?.UserId ?? 0,
                Username = account.Username,
                Email = account.Email,
                Role = (AccountRole)account.Role,
                Status = account.Status.HasValue ? (AccountStatus)account.Status.Value : null,
                FullName = account.User?.FullName ?? string.Empty,
                Phone = account.User?.Phone,
                Specialization = account.User?.Specialization,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt
            };
        }
    }
}
