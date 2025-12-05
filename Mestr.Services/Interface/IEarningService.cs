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
        Task<Earning> GetByUuidAsync(Guid uuid);
        Task<Earning> AddNewEarningAsync(Guid projectUuid, string description, decimal amount, DateTime date);
        Task<bool> DeleteAsync(Earning entity);
        Task<Earning> UpdateAsync(Earning entity);
    }
}
