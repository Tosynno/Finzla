using Finzla.Application.Contracts.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Finzla.Infrastructure.Persistence
{
    public abstract class BaseRepository<TContext, T> : IRepository<T>
        where T : class
        where TContext : DbContext
    {
        protected readonly TContext DbContext;

        protected BaseRepository(TContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => await DbContext.Set<T>().ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
            => await DbContext.Set<T>().Where(predicate).ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeString = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = DbContext.Set<T>();
            if (disableTracking) query = query.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(includeString)) query = query.Include(includeString);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return await orderBy(query).ToListAsync(cancellationToken);
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            List<Expression<Func<T, object>>>? includes = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = DbContext.Set<T>();
            if (disableTracking) query = query.AsNoTracking();
            if (includes != null) query = includes.Aggregate(query, (q, inc) => q.Include(inc));
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return await orderBy(query).ToListAsync(cancellationToken);
            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await DbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await DbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);

        public virtual async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => await DbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await DbContext.Set<T>().AddAsync(entity, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            DbContext.Set<T>().Remove(entity);
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => await DbContext.SaveChangesAsync(cancellationToken);
    }
}
