using System;
using System.Collections.Generic;
using System.Linq;
using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Services.Interface;
using Mestr.Data.Interface;
using Mestr.Data.Repository;

namespace Mestr.Services.Service
{
    public class ExpenseService : IExpenseService
    {
        private readonly IRepository<Expense> _expenseRepository;
        
        public ExpenseService()
        {
            _expenseRepository = new ExpenseRepository();
        }
        
        public Expense GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty) 
                throw new ArgumentException("UUID cannot be empty.", nameof(uuid));
            
            var expense = _expenseRepository.GetByUuid(uuid);
            if (expense == null)
                throw new ArgumentException("Expense not found.", nameof(uuid));
            
            return expense;
        }
        
        public Expense AddNewExpense(Guid projectUuid, string description, decimal amount, DateTime date, ExpenseCategory category)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty.", nameof(description));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            
            var expense = new Expense(Guid.NewGuid(), description, amount, date, category, false)
            {
                ProjectUuid = projectUuid
            };
            
            _expenseRepository.Add(expense);
            return expense;
        }
        
        public bool Delete(Expense entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
            
            _expenseRepository.Delete(entity.Uuid);
            return true;
        }
        
        public Expense Update(Expense entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
            
            var existing = _expenseRepository.GetByUuid(entity.Uuid);
            if (existing == null)
                throw new ArgumentException("Expense not found.", nameof(entity));
            
            // Update properties
            existing.Description = entity.Description;
            existing.Amount = entity.Amount;
            existing.Date = entity.Date;
            existing.Category = entity.Category;
            existing.IsAccepted = entity.IsAccepted;
            
            _expenseRepository.Update(existing);
            return existing;
        }
    }
}
