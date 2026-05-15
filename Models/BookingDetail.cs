using System;
using System.Collections.Generic;

namespace PetCareSystem.API.Models;

public partial class BookingDetail
{
    public long BookingDetailId { get; set; }

    public long BookingId { get; set; }

    public long ServiceId { get; set; }

    public int? Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal SubTotal { get; set; }

    public string? Note { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
