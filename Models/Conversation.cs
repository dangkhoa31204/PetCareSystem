using System;
using System.Collections.Generic;
using PetCareSystem.API.Enums;

namespace PetCareSystem.API.Models;

public partial class Conversation
{
    public long ConversationId { get; set; }

    public long UserId { get; set; }

    public long? DoctorId { get; set; }

    public long? PetId { get; set; }

    public int? Type { get; set; }

    public int? Status { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public virtual ICollection<LogAi> LogAis { get; set; } = new List<LogAi>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Pet? Pet { get; set; }

    public virtual User User { get; set; } = null!;
}
