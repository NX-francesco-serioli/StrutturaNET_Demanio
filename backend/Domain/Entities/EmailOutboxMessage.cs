namespace AdSPMdS.DemanioDigitale.Domain.Entities;

public class EmailOutboxMessage
{
    public Guid Id { get; set; }
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? Template { get; set; }
    public string? PayloadJson { get; set; }
    public string? CcJson { get; set; }
    public string? BccJson { get; set; }
    public string? ReplyTo { get; set; }
    public string? AttachmentsJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public string? LastError { get; set; }
}
