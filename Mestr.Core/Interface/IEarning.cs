using System;
using System.Collections.Generic;

namespace Mestr.Core.Interface
{
    public interface IEarning
    {
        Guid Uuid { get; }
        Guid ProjectUuid { get; set; }
        string Description { get; set; }
        decimal Amount { get; set; }
        DateTime Date { get; set; }
        bool IsPaid { get; set; }
        void MarkAsPaid(DateTime paymentDate);
    }
}
