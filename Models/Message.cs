using System;
using System.Collections.Generic;
using PetCareSystem.API.Enums;

namespace PetCareSystem.API.Models;

public partial class Message
{
    public long MessageId { get; set; }

    public long ConversationId { get; set; }

    public long SenderId { get; set; }

    public int? SenderType { get; set; }

    public int? MessageType { get; set; }

    public string? Content { get; set; }

    public string? AttachmentUrl { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
}
