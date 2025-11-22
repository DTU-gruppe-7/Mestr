using Mestr.Core.Enum;
using Mestr.Core.Interface;
using Mestr.Core.Model;
using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.EntityFrameworkCore;

namespace Mestr.Data.Repository
{
    public class EarningRepository : IRepository<Earning>
    {
        public void Add(Earning entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            dbContext.DatabaseLock.Wait();
            try
            {
                dbContext.Instance.Earnings.Add(entity);
                dbContext.Instance.SaveChanges();
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public Earning? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            dbContext.DatabaseLock.Wait();
            try
            {
                return dbContext.Instance.Earnings
                    .Include(e => e.Project)
                    .FirstOrDefault(e => e.Uuid == uuid);
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public IEnumerable<Earning> GetAll()
        {
            dbContext.DatabaseLock.Wait();
            try
            {
                return dbContext.Instance.Earnings
                    .Include(e => e.Project)
                    .AsNoTracking()
                    .ToList();
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public void Update(Earning entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            dbContext.DatabaseLock.Wait();
            try
            {
                dbContext.Instance.Earnings.Update(entity);
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
                var earning = dbContext.Instance.Earnings.FirstOrDefault(e => e.Uuid == uuid);
                if (earning != null)
                {
                    dbContext.Instance.Earnings.Remove(earning);
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