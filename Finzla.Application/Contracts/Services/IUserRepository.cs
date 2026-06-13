using Finzla.Domain.Entities;

namespace Finzla.Application.Contracts.Services
{
    public interface IUserRepository : IRepository<AppUser>
    {
        Task<AppUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<AppUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string username, string email, CancellationToken cancellationToken = default);
    }
}
