using Microsoft.AspNetCore.Identity;

namespace AdSPMdS.DemanioDigitale.Api.Data;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
}
