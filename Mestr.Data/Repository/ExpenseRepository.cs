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
                await context.SaveChangesAsync();
            }
        }

        public async Task<Expense?> GetByUuidAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                return await context.Expenses
                    .Include(e => e.Project)
                    .FirstOrDefaultAsync(e => e.Uuid == uuid);
            }
        }

        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            using (var context = new dbContext())
            {
                return await context.Expenses
                    .Include(e => e.Project)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task UpdateAsync(Expense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                // Fetch the existing entity from this context
                var existing = context.Expenses.FirstOrDefault(e => e.Uuid == entity.Uuid);
                
                if (existing != null)
                {
                    // Update properties
                    existing.Description = entity.Description;
                    existing.Amount = entity.Amount;
                    existing.Date = entity.Date;
                    existing.Category = entity.Category;
                    existing.IsAccepted = entity.IsAccepted;
                    existing.ProjectUuid = entity.ProjectUuid;
                    
                    await context.SaveChangesAsync();
                }
                else
                {
                    // If not found, throw exception
                    throw new InvalidOperationException($"Expense with UUID {entity.Uuid} not found.");
                }
            }
        }

        public async Task DeleteAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                var expense = context.Expenses.FirstOrDefault(e => e.Uuid == uuid);
                if (expense != null)
                {
                    context.Expenses.Remove(expense);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}