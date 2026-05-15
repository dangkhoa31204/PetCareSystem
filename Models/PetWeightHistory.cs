using System;
using System.Collections.Generic;

namespace PetCareSystem.API.Models;

public partial class PetWeightHistory
{
    public long WeightHistoryId { get; set; }

    public long PetId { get; set; }

    public decimal Weight { get; set; }

    public DateTime? RecordedAt { get; set; }

    public string? Note { get; set; }

    public virtual Pet Pet { get; set; } = null!;
}
