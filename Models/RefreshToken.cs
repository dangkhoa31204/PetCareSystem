using System;
using System.Collections.Generic;

namespace PetCareSystem.API.Models;

public partial class RefreshToken
{
    public long RefreshTokenId { get; set; }

    public long AccountId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiredAt { get; set; }

    public bool? IsRevoked { get; set; }

    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
