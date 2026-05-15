using System;
using System.Collections.Generic;
using PetCareSystem.API.Enums;

namespace PetCareSystem.API.Models;

public partial class Pet
{
    public long PetId { get; set; }

    public long UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Species { get; set; }

    public string? Breed { get; set; }

    public int? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Color { get; set; }

    public decimal? CurrentWeight { get; set; }

    public decimal? PreviousWeight { get; set; }

    public DateTime? WeightUpdatedAt { get; set; }

    public string? HealthStatus { get; set; }

    public string? AllergyInfo { get; set; }

    public string? VaccinationInfo { get; set; }

    public string? MedicalHistory { get; set; }

    public string? AvatarUrl { get; set; }

    public bool? IsNeutered { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<LogAi> LogAis { get; set; } = new List<LogAi>();

    public virtual ICollection<PetWeightHistory> PetWeightHistories { get; set; } = new List<PetWeightHistory>();

    public virtual User User { get; set; } = null!;
}
