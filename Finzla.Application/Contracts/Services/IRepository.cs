using System.Linq.Expressions;

namespace Finzla.Application.Contracts.Services
{
    
    public interface IRepository<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeString = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            List<Expression<Func<T, object>>>? includes = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default);

        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
