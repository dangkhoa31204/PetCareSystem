using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Enums;
using System.Security.Claims;

namespace PetCareSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PetController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public PetController(PetCareSystemContext context)
        {
            _context = context;
        }

        // GET: api/pet - Get all pets of current user
        [HttpGet]
        public async Task<ActionResult<List<PetDto>>> GetMyPets()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var pets = await _context.Pets
                .Where(p => p.UserId == userId)
                .Select(p => new PetDto
                {
                    PetId = p.PetId,
                    Name = p.Name,
                    Species = p.Species,
                    Breed = p.Breed,
                    Gender = p.Gender,
                    BirthDate = p.BirthDate,
                    Color = p.Color,
                    CurrentWeight = p.CurrentWeight,
                    HealthStatus = p.HealthStatus,
                    AvatarUrl = p.AvatarUrl,
                    IsNeutered = p.IsNeutered
                })
                .ToListAsync();

            return Ok(pets);
        }

        // GET: api/pet/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PetDto>> GetPet(long id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var pet = await _context.Pets
                .Where(p => p.PetId == id && p.UserId == userId)
                .FirstOrDefaultAsync();

            if (pet == null)
                return NotFound("Pet not found");

            var petDto = new PetDto
            {
                PetId = pet.PetId,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Gender = pet.Gender,
                BirthDate = pet.BirthDate,
                Color = pet.Color,
                CurrentWeight = pet.CurrentWeight,
                HealthStatus = pet.HealthStatus,
                AvatarUrl = pet.AvatarUrl,
                IsNeutered = pet.IsNeutered
            };

            return Ok(petDto);
        }

        // GET: api/pet/user/{userId} - Get all pets by user id
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<PetDto>>> GetPetsByUserId(long userId)
        {
            var pets = await _context.Pets
                .Where(p => p.UserId == userId)
                .Select(p => new PetDto
                {
                    PetId = p.PetId,
                    Name = p.Name,
                    Species = p.Species,
                    Breed = p.Breed,
                    Gender = p.Gender,
                    BirthDate = p.BirthDate,
                    Color = p.Color,
                    CurrentWeight = p.CurrentWeight,
                    HealthStatus = p.HealthStatus,
                    AvatarUrl = p.AvatarUrl,
                    IsNeutered = p.IsNeutered
                })
                .ToListAsync();

            return Ok(pets);
        }

        // POST: api/pet
        [HttpPost]
        public async Task<ActionResult<PetDto>> CreatePet(CreatePetDto petDto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var pet = new Pet
            {
                UserId = userId,
                Name = petDto.Name,
                Species = petDto.Species,
                Breed = petDto.Breed,
                Gender = petDto.Gender,
                BirthDate = petDto.BirthDate,
                Color = petDto.Color,
                CurrentWeight = petDto.CurrentWeight,
                HealthStatus = petDto.HealthStatus,
                AvatarUrl = petDto.AvatarUrl,
                IsNeutered = petDto.IsNeutered,
                CreatedAt = DateTime.UtcNow
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            var response = new PetDto
            {
                PetId = pet.PetId,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Gender = pet.Gender,
                BirthDate = pet.BirthDate,
                Color = pet.Color,
                CurrentWeight = pet.CurrentWeight,
                HealthStatus = pet.HealthStatus,
                AvatarUrl = pet.AvatarUrl,
                IsNeutered = pet.IsNeutered
            };

            return CreatedAtAction(nameof(GetPet), new { id = pet.PetId }, response);
        }

        // PUT: api/pet/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(long id, UpdatePetDto petDto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var pet = await _context.Pets
                .Where(p => p.PetId == id && p.UserId == userId)
                .FirstOrDefaultAsync();

            if (pet == null)
                return NotFound("Pet not found");

            pet.Name = petDto.Name ?? pet.Name;
            pet.Species = petDto.Species ?? pet.Species;
            pet.Breed = petDto.Breed ?? pet.Breed;
            pet.Gender = petDto.Gender ?? pet.Gender;
            pet.BirthDate = petDto.BirthDate ?? pet.BirthDate;
            pet.Color = petDto.Color ?? pet.Color;
            pet.CurrentWeight = petDto.CurrentWeight ?? pet.CurrentWeight;
            pet.HealthStatus = petDto.HealthStatus ?? pet.HealthStatus;
            pet.AvatarUrl = petDto.AvatarUrl ?? pet.AvatarUrl;
            pet.IsNeutered = petDto.IsNeutered ?? pet.IsNeutered;
            pet.UpdatedAt = DateTime.UtcNow;

            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();

            return Ok("Pet updated successfully");
        }

        // DELETE: api/pet/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(long id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var pet = await _context.Pets
                .Where(p => p.PetId == id && p.UserId == userId)
                .FirstOrDefaultAsync();

            if (pet == null)
                return NotFound("Pet not found");

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            return Ok("Pet deleted successfully");
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
