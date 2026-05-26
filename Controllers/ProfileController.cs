using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Dtos.Admin;
using PetCareSystem.API.Dtos.Common;
using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Dtos.Doctor;
using PetCareSystem.API.Dtos.Staff;
using PetCareSystem.API.Models;
using System.Security.Claims;

namespace PetCareSystem.API.Controllers
{
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public ProfileController(PetCareSystemContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var role = (Enums.AccountRole)user.Account.Role;
            return role switch
            {
                Enums.AccountRole.Customer => Ok(new CustomerProfileDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Account.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    AvatarUrl = user.AvatarUrl,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    IsProMember = user.IsProMember,
                    ProExpiredAt = user.ProExpiredAt
                }),
                Enums.AccountRole.Doctor => Ok(new DoctorProfileDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Account.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    AvatarUrl = user.AvatarUrl,
                    Specialization = user.Specialization
                }),
                Enums.AccountRole.Staff => Ok(new StaffProfileDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Account.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    AvatarUrl = user.AvatarUrl,
                    Department = user.Specialization
                }),
                Enums.AccountRole.Admin => Ok(new AdminProfileDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Account.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    AvatarUrl = user.AvatarUrl
                }),
                _ => Ok(new CustomerProfileDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Account.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    AvatarUrl = user.AvatarUrl,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    IsProMember = user.IsProMember,
                    ProExpiredAt = user.ProExpiredAt
                })
            };
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateDto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var role = (Enums.AccountRole)user.Account.Role;
            switch (role)
            {
                case Enums.AccountRole.Customer:
                    ApplyCustomerUpdate(user, updateDto);
                    break;
                case Enums.AccountRole.Doctor:
                    ApplyDoctorUpdate(user, updateDto);
                    break;
                case Enums.AccountRole.Staff:
                    ApplyStaffUpdate(user, updateDto);
                    break;
                case Enums.AccountRole.Admin:
                    ApplyAdminUpdate(user, updateDto);
                    break;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully");
        }


        private static void ApplyCustomerUpdate(User user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            if (dto.Phone != null)
            {
                user.Phone = dto.Phone;
            }

            if (dto.Address != null)
            {
                user.Address = dto.Address;
            }

            if (dto.AvatarUrl != null)
            {
                user.AvatarUrl = dto.AvatarUrl;
            }

            if (dto.DateOfBirth.HasValue)
            {
                user.DateOfBirth = dto.DateOfBirth.Value;
            }

            if (dto.Gender.HasValue)
            {
                user.Gender = dto.Gender.Value;
            }
        }

        private static void ApplyDoctorUpdate(User user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            if (dto.Phone != null)
            {
                user.Phone = dto.Phone;
            }

            if (dto.Address != null)
            {
                user.Address = dto.Address;
            }

            if (dto.AvatarUrl != null)
            {
                user.AvatarUrl = dto.AvatarUrl;
            }

            if (dto.Specialization != null)
            {
                user.Specialization = dto.Specialization;
            }
        }

        private static void ApplyStaffUpdate(User user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            if (dto.Phone != null)
            {
                user.Phone = dto.Phone;
            }

            if (dto.Address != null)
            {
                user.Address = dto.Address;
            }

            if (dto.AvatarUrl != null)
            {
                user.AvatarUrl = dto.AvatarUrl;
            }

            if (dto.Department != null)
            {
                user.Specialization = dto.Department;
            }
        }

        private static void ApplyAdminUpdate(User user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            if (dto.Phone != null)
            {
                user.Phone = dto.Phone;
            }

            if (dto.Address != null)
            {
                user.Address = dto.Address;
            }

            if (dto.AvatarUrl != null)
            {
                user.AvatarUrl = dto.AvatarUrl;
            }
        }

        private long GetUserIdFromClaims()
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !long.TryParse(accountIdClaim.Value, out var accountId))
            {
                return 0;
            }

            return _context.Users
                .Where(u => u.AccountId == accountId)
                .Select(u => u.UserId)
                .FirstOrDefault();
        }
    }
}
