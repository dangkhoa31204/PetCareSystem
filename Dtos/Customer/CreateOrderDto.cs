using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Customer
{
    public class CreateOrderDto
    {
        [Required]
        public List<OrderItemDto> Items { get; set; } = new();

        public long? BookingId { get; set; }

        public string? ShippingAddress { get; set; }

        public decimal? DiscountAmount { get; set; }
    }

    public class OrderItemDto
    {
        [Required]
        public long ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}
