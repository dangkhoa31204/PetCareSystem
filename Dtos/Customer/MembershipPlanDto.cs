namespace PetCareSystem.API.Dtos.Customer;

public class MembershipPlanDto
{
    public long MembershipPlanId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
