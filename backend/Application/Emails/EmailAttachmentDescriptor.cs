namespace AdSPMdS.DemanioDigitale.Application.Emails;

public record EmailAttachmentDescriptor(
    string FileName,
    string ContentType,
    string StorageKey,
    long Size);
