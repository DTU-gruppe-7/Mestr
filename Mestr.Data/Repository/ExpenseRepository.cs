using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Mestr.Core.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mestr.Data.Repository
{
    public class ExpenseRepository : IRepository<Expense>
    {
        public async Task AddAsync(Expense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                context.Expenses.Add(entity);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<Expense?> GetByUuidAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                return await context.Expenses
                    .Include(e => e.Project)
                    .FirstOrDefaultAsync(e => e.Uuid == uuid)
                    .ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            using (var context = new dbContext())
            {
                return await context.Expenses
                    .Include(e => e.Project)
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task UpdateAsync(Expense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                var existing = await context.Expenses
                    .FirstOrDefaultAsync(e => e.Uuid == entity.Uuid)
                    .ConfigureAwait(false);
                
                if (existing != null)
                {
                    existing.Description = entity.Description;
                    existing.Amount = entity.Amount;
                    existing.Date = entity.Date;
                    existing.Category = entity.Category;
                    existing.IsAccepted = entity.IsAccepted;
                    existing.ProjectUuid = entity.ProjectUuid;
                    
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
                else
                {
                    throw new InvalidOperationException($"Expense with UUID {entity.Uuid} not found.");
                }
            }
        }

        public async Task DeleteAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                var expense = await context.Expenses
                    .FirstOrDefaultAsync(e => e.Uuid == uuid)
                    .ConfigureAwait(false);
                if (expense != null)
                {
                    context.Expenses.Remove(expense);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
    }
}