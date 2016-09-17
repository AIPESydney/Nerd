using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Nerd.Api.Interfaces
{
    public interface IGenericRepository<TEntity>
    {
        Task<TEntity> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate);

        IQueryable<TEntity> SearchFor(Expression<Func<TEntity, bool>> predicate);

        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "");
        
        IQueryable<TEntity> GetAll();

        Task EditAsync(TEntity entity);

        Task InsertAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);
        Task SoftDeleteAsync(TEntity entity);
    }
}