using Mestr.Core.Model;
using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.EntityFrameworkCore;

namespace Mestr.Data.Repository
{
    public class ClientRepository : IRepository<Client>
    {
        public void Add(Client entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            dbContext.DatabaseLock.Wait();
            try
            {
                dbContext.Instance.Clients.Add(entity);
                dbContext.Instance.SaveChanges();
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public Client? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            dbContext.DatabaseLock.Wait();
            try
            {
                return dbContext.Instance.Clients
                    .Include(c => c.Projects)
                    .FirstOrDefault(c => c.Uuid == uuid);
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public IEnumerable<Client> GetAll()
        {
            dbContext.DatabaseLock.Wait();
            try
            {
                return dbContext.Instance.Clients
                    .Include(c => c.Projects)
                    .AsNoTracking()
                    .ToList();
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public void Update(Client entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            dbContext.DatabaseLock.Wait();
            try
            {
                dbContext.Instance.Clients.Update(entity);
                dbContext.Instance.SaveChanges();
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            dbContext.DatabaseLock.Wait();
            try
            {
                var client = dbContext.Instance.Clients.FirstOrDefault(c => c.Uuid == uuid);
                if (client != null)
                {
                    dbContext.Instance.Clients.Remove(client);
                    dbContext.Instance.SaveChanges();
                }
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }
    }
}