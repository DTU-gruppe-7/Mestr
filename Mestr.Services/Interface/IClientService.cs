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
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client> CreateClientAsync(string companyName, string contactName, string email, string phoneNumber,
                                  string address, string postalAddress, string city, string? cvr = null);

        Task<Client?> GetClientByUuidAsync(Guid uuid);

        Task UpdateClientAsync(Client client);

        Task DeleteClientAsync(Guid clientId);
    }
}
