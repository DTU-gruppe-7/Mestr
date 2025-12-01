using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        
        public ExpenseService(IRepository<Expense> expenseRepo)
        {
            _expenseRepository = expenseRepo ?? throw new ArgumentNullException(nameof(expenseRepo));
        }
        
        public async Task<Expense> GetByUuidAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) 
                throw new ArgumentException("UUID cannot be empty.", nameof(uuid));
            
            var expense = await _expenseRepository.GetByUuidAsync(uuid);
            if (expense == null)
                throw new ArgumentException("Expense not found.", nameof(uuid));
            
            return expense;
        }
        
        public async Task<Expense> AddNewExpenseAsync(Guid projectUuid, string description, decimal amount, DateTime date, ExpenseCategory category)
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
            
            await _expenseRepository.AddAsync(expense);
            return expense;
        }
        
        public async Task<bool> DeleteAsync(Expense entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
            
            await _expenseRepository.DeleteAsync(entity.Uuid);
            return true;
        }
        
        public async Task<Expense> UpdateAsync(Expense entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
            
            await _expenseRepository.UpdateAsync(entity);
            
            var updatedExpense = await _expenseRepository.GetByUuidAsync(entity.Uuid);
            return updatedExpense 
                ?? throw new InvalidOperationException("Failed to retrieve updated expense.");
        }
    }
}
