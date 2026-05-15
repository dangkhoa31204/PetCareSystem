using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Customer
{
    public class CreateBookingDto
    {
        [Required]
        public long PetId { get; set; }

        [Required]
        public DateOnly BookingDate { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Required]
        public List<BookingDetailItemDto> Services { get; set; } = new();

        public string? Note { get; set; }
    }

    public class BookingDetailItemDto
    {
        [Required]
        public long ServiceId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}
