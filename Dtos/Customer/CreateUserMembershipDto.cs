namespace PetCareSystem.API.Dtos.Customer;

public class CreateUserMembershipDto
{
    public long MembershipPlanId { get; set; }

    public long? PaymentId { get; set; }
}
