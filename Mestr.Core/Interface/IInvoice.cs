using Mestr.Core.Enum;
using Mestr.Core.Model;
using System;

namespace Mestr.Core.Interface
{
    public interface IInvoice
    {
        Guid Uuid { get; }
        Guid ProjectUuid { get; }
        string InvoiceNumber { get; set; }
        DateTime InvoiceDate { get; set; }
        DateTime DueDate { get; set; }
        decimal SubTotal { get; set; }
        decimal VatPercentage { get; set; }
        decimal VatAmount { get; set; }
        decimal TotalAmount { get; set; }
        InvoiceStatus Status { get; set; }
        string FilePath { get; set; }
        IProject Project { get; set; }
    }
}
