using PetHaven.Domain.Common;
using System.Linq.Expressions;

namespace PetHaven.Application.Interfaces;
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);

    Task<List<T>> GetAllAsync();

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    Task AddAsync(T entity);

    void Update(T entity);

    void Delete(T entity);
}