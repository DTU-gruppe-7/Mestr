using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mestr.Services.Interface
{
    public interface IClientService
    {
        IEnumerable<Core.Model.Client> GetAllClients();
    }
}
