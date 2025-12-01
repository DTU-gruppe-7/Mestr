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
        public async Task AddAsync(Earning entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                context.Earnings.Add(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Earning?> GetByUuidAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                return await context.Earnings
                    .Include(e => e.Project)
                    .FirstOrDefaultAsync(e => e.Uuid == uuid);
            }
        }

        public async Task<IEnumerable<Earning>> GetAllAsync()
        {
            using (var context = new dbContext())
            {
                return await context.Earnings
                    .Include(e => e.Project)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task UpdateAsync(Earning entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                // Fetch the existing entity from this context
                var existing = await context.Earnings.FirstOrDefaultAsync(e => e.Uuid == entity.Uuid);
                
                if (existing != null)
                {
                    // Update properties
                    existing.Description = entity.Description;
                    existing.Amount = entity.Amount;
                    existing.Date = entity.Date;
                    existing.IsPaid = entity.IsPaid;
                    existing.ProjectUuid = entity.ProjectUuid;
                    
                    await context.SaveChangesAsync();
                }
                else
                {
                    // If not found, throw exception
                    throw new InvalidOperationException($"Earning with UUID {entity.Uuid} not found.");
                }
            }
        }

        public async Task DeleteAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                var earning = await context.Earnings.FirstOrDefaultAsync(e => e.Uuid == uuid);
                if (earning != null)
                {
                    context.Earnings.Remove(earning);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}