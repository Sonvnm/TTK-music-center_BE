using System.Linq.Expressions;

namespace HMZ.Service.Services
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> AsQueryable();
        IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression, bool asNoTracking = true);
        IQueryable<T> ToPagedList(int pageNumber, int pageSize);
        Task<T> GetByIdAsync(Guid id);
        Task Add(T entity);
        Task AddRange(List<T> list);
        bool Update(T entity);
        bool UpdateRange(List<T> entities);
        void Delete(T entity, bool? isActive);
        void DeleteRange(List<T> entities, bool? isActive = false); // isActive = true: soft delete, isActive = false: hard delete
    }
}
