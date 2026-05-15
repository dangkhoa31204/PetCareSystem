namespace PetCareSystem.API.Dtos.Customer
{
    public class OrderItemResponseDto
    {
        public long OrderItemId { get; set; }

        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal SubTotal { get; set; }
    }

    public class OrderResponseDto
    {
        public long OrderId { get; set; }

        public string? OrderCode { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal FinalAmount { get; set; }

        public int? OrderType { get; set; }

        public int? Status { get; set; }

        public int? PaymentStatus { get; set; }

        public string? ShippingAddress { get; set; }

        public DateTime? CreatedAt { get; set; }

        public List<OrderItemResponseDto> Items { get; set; } = new();
    }
}
