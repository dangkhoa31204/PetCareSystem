using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Dtos.Staff;
using PetCareSystem.API.Enums;
using System.Security.Claims;

namespace PetCareSystem.API.Controllers
{
    [Route("api/staff/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff")]
    public class PetManagementController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public PetManagementController(PetCareSystemContext context)
        {
            _context = context;
        }

        // GET: api/staff/petmanagement - Get all pets
        [HttpGet]
        public async Task<ActionResult<List<PetDetailDto>>> GetAllPets([FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 10)
        {
            var pageNum = pageNumber ?? 1;
            var pageSz = pageSize ?? 10;

            var pets = await _context.Pets
                .Include(p => p.User)
                .Include(p => p.PetWeightHistories)
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .Skip((pageNum - 1) * pageSz)
                .Take(pageSz)
                .ToListAsync();

            var petDtos = pets.Select(p => MapToPetDetailDto(p)).ToList();
            return Ok(petDtos);
        }

        // GET: api/staff/petmanagement/{petId}
        [HttpGet("{petId}")]
        public async Task<ActionResult<PetDetailDto>> GetPetDetail(long petId)
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                .Include(p => p.PetWeightHistories.OrderByDescending(wh => wh.RecordedAt))
                .FirstOrDefaultAsync(p => p.PetId == petId);

            if (pet == null)
                return NotFound("Pet not found");

            return Ok(MapToPetDetailDto(pet));
        }

        // PUT: api/staff/petmanagement/{petId}/health - Update pet health info
        [HttpPut("{petId}/health")]
        public async Task<IActionResult> UpdatePetHealth(long petId, UpdatePetHealthDto updatePetHealthDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var pet = await _context.Pets
                .Include(p => p.PetWeightHistories)
                .FirstOrDefaultAsync(p => p.PetId == petId);

            if (pet == null)
                return NotFound("Pet not found");

            // Update health information
            if (updatePetHealthDto.HealthStatus != null)
                pet.HealthStatus = updatePetHealthDto.HealthStatus;

            if (updatePetHealthDto.AllergyInfo != null)
                pet.AllergyInfo = updatePetHealthDto.AllergyInfo;

            if (updatePetHealthDto.VaccinationInfo != null)
                pet.VaccinationInfo = updatePetHealthDto.VaccinationInfo;

            if (updatePetHealthDto.MedicalHistory != null)
                pet.MedicalHistory = updatePetHealthDto.MedicalHistory;

            if (updatePetHealthDto.IsNeutered.HasValue)
                pet.IsNeutered = updatePetHealthDto.IsNeutered;

            // Update weight if provided
            if (updatePetHealthDto.CurrentWeight.HasValue)
            {
                pet.PreviousWeight = pet.CurrentWeight;
                pet.CurrentWeight = updatePetHealthDto.CurrentWeight.Value;
                pet.WeightUpdatedAt = DateTime.UtcNow;

                // Add to weight history
                var weightHistory = new PetWeightHistory
                {
                    PetId = petId,
                    Weight = updatePetHealthDto.CurrentWeight.Value,
                    RecordedAt = DateTime.UtcNow,
                    Note = updatePetHealthDto.WeightHistoryNote
                };
                pet.PetWeightHistories.Add(weightHistory);
            }

            pet.UpdatedAt = DateTime.UtcNow;

            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();

            return Ok("Pet health information updated successfully");
        }

        // GET: api/staff/petmanagement/{petId}/weight-history
        [HttpGet("{petId}/weight-history")]
        public async Task<ActionResult<List<PetWeightHistoryDto>>> GetPetWeightHistory(long petId)
        {
            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.PetId == petId);

            if (pet == null)
                return NotFound("Pet not found");

            var weightHistory = await _context.PetWeightHistories
                .Where(wh => wh.PetId == petId)
                .OrderByDescending(wh => wh.RecordedAt)
                .Select(wh => new PetWeightHistoryDto
                {
                    WeightHistoryId = wh.WeightHistoryId,
                    Weight = wh.Weight,
                    RecordedAt = wh.RecordedAt,
                    Note = wh.Note
                })
                .ToListAsync();

            return Ok(weightHistory);
        }

        // POST: api/staff/petmanagement/{petId}/weight-history
        [HttpPost("{petId}/weight-history")]
        public async Task<ActionResult<PetWeightHistoryDto>> RecordPetWeight(long petId, UpdatePetHealthDto updatePetHealthDto)
        {
            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.PetId == petId);

            if (pet == null)
                return NotFound("Pet not found");

            if (!updatePetHealthDto.CurrentWeight.HasValue)
                return BadRequest("Weight is required");

            var previousWeight = pet.CurrentWeight;
            pet.PreviousWeight = previousWeight;
            pet.CurrentWeight = updatePetHealthDto.CurrentWeight.Value;
            pet.WeightUpdatedAt = DateTime.UtcNow;
            pet.UpdatedAt = DateTime.UtcNow;

            var weightHistory = new PetWeightHistory
            {
                PetId = petId,
                Weight = updatePetHealthDto.CurrentWeight.Value,
                RecordedAt = DateTime.UtcNow,
                Note = updatePetHealthDto.WeightHistoryNote
            };

            _context.PetWeightHistories.Add(weightHistory);
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();

            var weightHistoryDto = new PetWeightHistoryDto
            {
                WeightHistoryId = weightHistory.WeightHistoryId,
                Weight = weightHistory.Weight,
                RecordedAt = weightHistory.RecordedAt,
                Note = weightHistory.Note
            };

            return CreatedAtAction(nameof(GetPetWeightHistory), new { petId = petId }, weightHistoryDto);
        }

        private PetDetailDto MapToPetDetailDto(Pet pet)
        {
            return new PetDetailDto
            {
                PetId = pet.PetId,
                UserId = pet.UserId,
                OwnerName = pet.User?.FullName ?? string.Empty,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Gender = pet.Gender,
                BirthDate = pet.BirthDate,
                Color = pet.Color,
                CurrentWeight = pet.CurrentWeight,
                PreviousWeight = pet.PreviousWeight,
                WeightUpdatedAt = pet.WeightUpdatedAt,
                HealthStatus = pet.HealthStatus,
                AllergyInfo = pet.AllergyInfo,
                VaccinationInfo = pet.VaccinationInfo,
                MedicalHistory = pet.MedicalHistory,
                AvatarUrl = pet.AvatarUrl,
                IsNeutered = pet.IsNeutered,
                CreatedAt = pet.CreatedAt,
                UpdatedAt = pet.UpdatedAt,
                WeightHistory = pet.PetWeightHistories?
                    .OrderByDescending(wh => wh.RecordedAt)
                    .Select(wh => new PetWeightHistoryDto
                    {
                        WeightHistoryId = wh.WeightHistoryId,
                        Weight = wh.Weight,
                        RecordedAt = wh.RecordedAt,
                        Note = wh.Note
                    })
                    .ToList() ?? new List<PetWeightHistoryDto>()
            };
        }
    }
}
