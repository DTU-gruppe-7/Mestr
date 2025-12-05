using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mestr.Data.Interface
{
    public interface IRepository<T>
    {
        Task AddAsync(T entity);
        Task<T?> GetByUuidAsync(Guid uuid);
        Task<IEnumerable<T>> GetAllAsync();
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid uuid);
    }
}
