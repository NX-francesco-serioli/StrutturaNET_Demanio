using AdSPMdS.DemanioDigitale.Domain.Entities;

namespace AdSPMdS.DemanioDigitale.Application.Repositories;

public interface IUserPermissionRepository
{
    Task AddAsync(UserPermission permission, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserPermission>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken);
}
