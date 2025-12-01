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
        public void Add(Expense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                context.Expenses.Add(entity);
                context.SaveChanges();
            }
        }

        public Expense? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                return context.Expenses
                    .Include(e => e.Project)
                    .FirstOrDefault(e => e.Uuid == uuid);
            }
        }

        public IEnumerable<Expense> GetAll()
        {
            using (var context = new dbContext())
            {
                return context.Expenses
                    .Include(e => e.Project)
                    .AsNoTracking()
                    .ToList();
            }
        }

        public void Update(Expense entity)
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
                    
                    context.SaveChanges();
                }
                else
                {
                    // If not found, throw exception
                    throw new InvalidOperationException($"Expense with UUID {entity.Uuid} not found.");
                }
            }
        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                var expense = context.Expenses.FirstOrDefault(e => e.Uuid == uuid);
                if (expense != null)
                {
                    context.Expenses.Remove(expense);
                    context.SaveChanges();
                }
            }
        }
    }
}