using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Data.Repository;
using Mestr.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mestr.Services.Service
{
    public class ClientService : IClientService
    {
        private readonly IRepository<Client> _clientRepository;

        public ClientService(IRepository<Client> clientRepo)
        {
            _clientRepository = clientRepo ?? throw new ArgumentNullException(nameof(clientRepo));
        }

        public async Task<Client> CreateClientAsync(string companyName, string contactName, string email, string phoneNumber, string address,
                                   string postalAddress, string city, string? cvr = null)
        {
            // Factory method handles all validation
            var newClient = Client.Create(
                Guid.NewGuid(),
                companyName,
                contactName,
                email,
                phoneNumber,
                address ?? string.Empty,
                postalAddress ?? string.Empty,
                city ?? string.Empty,
                cvr
            );

            await _clientRepository.AddAsync(newClient).ConfigureAwait(false);

            return newClient;
        }

        public async Task<Client?> GetClientByUuidAsync(Guid uuid)
        {
            return await _clientRepository.GetByUuidAsync(uuid).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _clientRepository.GetAllAsync().ConfigureAwait(false);
        }

        public async Task UpdateClientAsync(Client client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Client must not be null.");
            }
            await _clientRepository.UpdateAsync(client).ConfigureAwait(false);
        }

        public async Task DeleteClientAsync(Guid clientId)
        {
            var client = await _clientRepository.GetByUuidAsync(clientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new ArgumentException("Client not found.", nameof(clientId));
            }
            if (client.Projects?.Count > 0)
            {
                throw new InvalidOperationException($"Kunden kan ikke slettes fordi den har {client.Projects.Count} projekt(er) forbundet.");
            }
            await _clientRepository.DeleteAsync(clientId).ConfigureAwait(false);
        }
    }
}