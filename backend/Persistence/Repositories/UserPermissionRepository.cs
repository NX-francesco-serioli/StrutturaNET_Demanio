using AdSPMdS.DemanioDigitale.Application.Repositories;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdSPMdS.DemanioDigitale.Persistence.Repositories;

public class UserPermissionRepository : IUserPermissionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserPermissionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserPermission permission, CancellationToken cancellationToken)
    {
        _dbContext.UserPermissions.Add(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserPermission>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserPermissions
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
