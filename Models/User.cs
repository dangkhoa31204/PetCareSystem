using System;
using System.Collections.Generic;
using PetCareSystem.API.Enums;

namespace PetCareSystem.API.Models;

public partial class User
{
    public long UserId { get; set; }

    public long AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? AvatarUrl { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int? Gender { get; set; }

    public bool? IsProMember { get; set; }

    public DateTime? ProExpiredAt { get; set; }

    public string? EmergencyContact { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Specialization { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Booking> DoctorBookings { get; set; } = new List<Booking>();

    public virtual ICollection<Conversation> CustomerConversations { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> DoctorConversations { get; set; } = new List<Conversation>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<LogAi> LogAis { get; set; } = new List<LogAi>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public virtual ICollection<UserMembership> UserMemberships { get; set; } = new List<UserMembership>();
}
