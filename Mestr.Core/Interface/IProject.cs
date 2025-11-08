using Mestr.Core.Enum;
using Mestr.Core.Model;
using System;
using System.Collections.Generic;

namespace Mestr.Core.Interface
{
    public interface IProject
    {
        Guid Uuid { get; }
        string Name { get; set; }
        DateTime CreatedDate { get; set; }
        DateTime StartDate { get; set; }
        DateTime? EndDate { get; set; }
        string Description { get; set; }
        ProjectStatus Status { get; set; }
        IList<IExpense> Expenses { get; set; }
        IList<IEarning> Earnings { get; set; }
        bool IsFinished();
    }
}
