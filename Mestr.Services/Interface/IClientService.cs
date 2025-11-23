using Mestr.Core.Model;
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
        Client CreateClient(string? companyName, string contactName, string email, string phoneNumber,
                                  string address, string postalAddress, string city, string? cvr = null);

        Client? GetClientByUuid(Guid uuid);

        void UpdateClient(Client client);

        void DeleteClient(Guid clientId);
    }
}
