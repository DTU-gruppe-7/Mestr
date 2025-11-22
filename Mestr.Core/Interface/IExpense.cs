using Mestr.Core.Enum;
using Mestr.Core.Model;
using System;

namespace Mestr.Core.Interface
{
    public interface IExpense
    {
        Guid Uuid { get; }
        string Description { get; set; }
        decimal Amount { get; set; }
        DateTime Date { get; set; }
        ExpenseCategory Category { get; set; }
        bool IsAccepted { get; set; }
        void Accept();
    }
}
