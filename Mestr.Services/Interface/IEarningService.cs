using Mestr.Core.Enum;
using Mestr.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mestr.Services.Interface
{
    public interface IEarningService
    {
        Earning GetByUuid(Guid uuid);
        List<Earning> GetAllByProjectUuid(Guid projektUuid);
        Earning AddNewEarning(Guid projectUuid, string description, decimal amount, DateTime date);
        bool Delete(Earning entity);
        Earning Update(Earning entity);
    }
}
