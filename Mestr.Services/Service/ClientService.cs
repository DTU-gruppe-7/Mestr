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
            if (string.IsNullOrWhiteSpace(contactName))
            {
                throw new ArgumentException("Contact name cannot be null or empty.", nameof(contactName));
            }


            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));
            }

            var newClient = new Client(
                Guid.NewGuid(),
                companyName,
                contactName,
                email,
                phoneNumber,
                address,
                postalAddress,
                city,
                cvr!
            );

            await _clientRepository.AddAsync(newClient);

            return newClient;
        }

        public async Task<Client?> GetClientByUuidAsync(Guid uuid)
        {
            return await _clientRepository.GetByUuidAsync(uuid);
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _clientRepository.GetAllAsync();
        }

        public async Task UpdateClientAsync(Client client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Client must not be null.");
            }
            await _clientRepository.UpdateAsync(client);
        }

        public async Task DeleteClientAsync(Guid clientId)
        {
            var client = await _clientRepository.GetByUuidAsync(clientId);
            if (client == null)
            {
                throw new ArgumentException("Client not found.", nameof(clientId));
            }
            if (client.Projects?.Count > 0)
            {
                throw new InvalidOperationException($"Kunden kan ikke slettes fordi den har {client.Projects.Count} projekt(er) forbundet.");
            }
            await _clientRepository.DeleteAsync(clientId);
        }

    }
}