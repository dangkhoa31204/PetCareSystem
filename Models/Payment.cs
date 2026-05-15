using System;
using System.Collections.Generic;
using PetCareSystem.API.Enums;

namespace PetCareSystem.API.Models;

public partial class Payment
{
    public long PaymentId { get; set; }

    public long OrderId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionCode { get; set; }

    public decimal Amount { get; set; }

    public int? PaymentStatus { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
