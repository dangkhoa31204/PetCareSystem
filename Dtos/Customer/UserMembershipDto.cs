namespace PetCareSystem.API.Dtos.Customer;

public class UserMembershipDto
{
    public long UserMembershipId { get; set; }

    public long UserId { get; set; }

    public long MembershipPlanId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal PricePaid { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public MembershipPlanDto? MembershipPlan { get; set; }
}
