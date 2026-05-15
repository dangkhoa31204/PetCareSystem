namespace PetCareSystem.API.Dtos.Customer;

public class UserMembershipResponseDto
{
    public long UserMembershipId { get; set; }

    public long UserId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public string StatusText { get; set; } = null!;

    public MembershipPlanDto MembershipPlan { get; set; } = null!;
}
