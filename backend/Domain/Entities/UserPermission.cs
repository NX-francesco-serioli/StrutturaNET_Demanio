namespace AdSPMdS.DemanioDigitale.Domain.Entities;

public class UserPermission
{
    public Guid Id { get; set; }

    public string Permission { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = default!;
}
