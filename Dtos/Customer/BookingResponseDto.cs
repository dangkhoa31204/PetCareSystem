namespace PetCareSystem.API.Dtos.Customer
{
    public class BookingDetailDto
    {
        public long BookingDetailId { get; set; }

        public long ServiceId { get; set; }

        public string ServiceName { get; set; }

        public int? Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal SubTotal { get; set; }

        public string? Note { get; set; }
    }

    public class BookingResponseDto
    {
        public long BookingId { get; set; }

        public string? BookingCode { get; set; }

        public long PetId { get; set; }

        public string PetName { get; set; }

        public DateOnly BookingDate { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? Status { get; set; }

        public string? Note { get; set; }

        public decimal? TotalPrice { get; set; }

        public DateTime? CreatedAt { get; set; }

        public List<BookingDetailDto> Services { get; set; } = new();
    }
}
