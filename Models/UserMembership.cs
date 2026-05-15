using System;
using System.Collections.Generic;

namespace PetCareSystem.API.Models;

public partial class UserMembership
{
    public long UserMembershipId { get; set; }

    public long UserId { get; set; }

    public long MembershipPlanId { get; set; }

    public long? PaymentId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal PricePaid { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual MembershipPlan MembershipPlan { get; set; } = null!;

    public virtual Payment? Payment { get; set; }

    public virtual User User { get; set; } = null!;
}
