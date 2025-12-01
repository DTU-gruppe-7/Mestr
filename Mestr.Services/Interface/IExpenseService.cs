using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mestr.Core.Enum;
using Mestr.Core.Model;

namespace Mestr.Services.Interface
{
    public interface IExpenseService
    {
        Task<Expense> GetByUuidAsync(Guid uuid);
        Task<Expense> AddNewExpenseAsync(Guid projectUuid, string description, decimal amount, DateTime date, ExpenseCategory category);
        Task<bool> DeleteAsync(Expense entity);
        Task<Expense> UpdateAsync(Expense entity);
    }
}
