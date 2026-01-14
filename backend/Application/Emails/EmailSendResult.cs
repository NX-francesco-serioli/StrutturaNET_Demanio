namespace AdSPMdS.DemanioDigitale.Application.Emails;

public record EmailSendResult(bool Success, bool Skipped, string? Error);
