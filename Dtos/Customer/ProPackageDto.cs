namespace PetCareSystem.API.Dtos.Customer
{
    public class ProPackageDto
    {
        public int PackageType { get; set; }

        public string PackageName { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public List<string> Benefits { get; set; } = new();

        public List<int> AvailableServiceCategories { get; set; } = new();
    }
}

