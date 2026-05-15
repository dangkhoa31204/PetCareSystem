using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Staff
{
    public class UpdateBookingStatusDto
    {
        [Required]
        public int Status { get; set; }

        public string? Note { get; set; }
    }
}
