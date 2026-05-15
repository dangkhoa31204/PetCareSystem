using System;
using System.Collections.Generic;
using PetCareSystem.API.Enums;

namespace PetCareSystem.API.Models;

public partial class Order
{
    public long OrderId { get; set; }

    public long UserId { get; set; }

    public long? BookingId { get; set; }

    public string? OrderCode { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public int? OrderType { get; set; }

    public int? Status { get; set; }

    public int? PaymentStatus { get; set; }

    public string? ShippingAddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Payment? Payment { get; set; }

    public virtual User User { get; set; } = null!;
}
