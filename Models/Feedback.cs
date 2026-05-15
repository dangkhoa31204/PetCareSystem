using System;
using System.Collections.Generic;

namespace PetCareSystem.API.Models;

public partial class Feedback
{
    public long FeedbackId { get; set; }

    public long UserId { get; set; }

    public long? BookingId { get; set; }

    public long? ProductId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public string? Reply { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User User { get; set; } = null!;
}
