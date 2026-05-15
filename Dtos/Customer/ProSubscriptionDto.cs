using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Customer
{
    public class SubscribeProPackageDto
    {
        [Required]
        [Range(1, 2)]
        public int PackageType { get; set; }
    }

    public class ProSubscriptionResponseDto
    {
        public int PackageType { get; set; }

        public string PackageName { get; set; }

        public bool IsActive { get; set; }

        public decimal ServiceDiscount { get; set; }

        public decimal ProductDiscount { get; set; }

        public List<string> Benefits { get; set; } = new();

        public List<int> AvailableServiceCategories { get; set; } = new();
    }

    public class ProMemberBenefitsDto
    {
        public int PackageType { get; set; }

        public string PackageName { get; set; }

        public bool IsActive { get; set; }

        public decimal ServiceDiscount { get; set; }

        public decimal ProductDiscount { get; set; }

        public List<string> Benefits { get; set; } = new();
    }
}
