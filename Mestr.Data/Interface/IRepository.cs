using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mestr.Data.Interface
{
    internal interface IRepository<T>
    {
        void Add(T entity);
        T GetByID(int uuid);
        IEnumerable<T> GetAll();
        void Update(T entity);
        void Delete(int uuid);
    }
}
