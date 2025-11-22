using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Core.Enum;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;

namespace Mestr.Services.Interface
{
    public interface IPdfService
    {
        public byte[] GeneratePdfInvoice(Project project);
    }
}
