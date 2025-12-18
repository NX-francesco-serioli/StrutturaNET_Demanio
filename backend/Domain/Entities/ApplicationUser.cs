using Microsoft.AspNetCore.Identity;

namespace AdSPMdS.DemanioDigitale.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
}
