using System;
using System.Collections.Generic;

namespace PetCareSystem.API.Models;

public partial class LogAi
{
    public long LogAiid { get; set; }

    public long UserId { get; set; }

    public long? PetId { get; set; }

    public long? ConversationId { get; set; }

    public string? Prompt { get; set; }

    public string? Response { get; set; }

    public string? ModelName { get; set; }

    public int? TokensUsed { get; set; }

    public int? ResponseTimeMs { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Conversation? Conversation { get; set; }

    public virtual Pet? Pet { get; set; }

    public virtual User User { get; set; } = null!;
}
