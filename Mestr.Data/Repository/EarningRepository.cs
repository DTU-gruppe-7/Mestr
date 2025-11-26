using Mestr.Core.Model;
using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mestr.Data.Repository
{
    public class EarningRepository : IRepository<Earning>
    {
        public void Add(Earning entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                context.Earnings.Add(entity);
                context.SaveChanges();
            }
        }

        public Earning? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                return context.Earnings
                    .Include(e => e.Project)
                    .FirstOrDefault(e => e.Uuid == uuid);
            }
        }

        public IEnumerable<Earning> GetAll()
        {
            using (var context = new dbContext())
            {
                return context.Earnings
                    .Include(e => e.Project)
                    .AsNoTracking()
                    .ToList();
            }
        }

        public void Update(Earning entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                context.Earnings.Update(entity);
                context.SaveChanges();
            }
        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                var earning = context.Earnings.FirstOrDefault(e => e.Uuid == uuid);
                if (earning != null)
                {
                    context.Earnings.Remove(earning);
                    context.SaveChanges();
                }
            }
        }
    }
}