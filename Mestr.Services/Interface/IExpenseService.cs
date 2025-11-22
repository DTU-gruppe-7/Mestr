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
        Expense GetByUuid(Guid uuid);
        Expense AddNewExpense(Guid projectUuid, string description, decimal amount, DateTime date, ExpenseCategory category);
        bool Delete(Expense entity);
        Expense Update(Expense entity);
    }
}
