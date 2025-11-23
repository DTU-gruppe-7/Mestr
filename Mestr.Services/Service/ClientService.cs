using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Data.Repository;
using Mestr.Services.Interface;
using System;
using System.Collections.Generic;

namespace Mestr.Services.Service
{
    public class ClientService : IClientService
    {
        private readonly IRepository<Client> _clientRepository;

        public ClientService()
        {
            _clientRepository = new ClientRepository();
        }

        public Client CreateClient(string companyName, string contactName, string email, string phoneNumber, string address,
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

            _clientRepository.Add(newClient);

            return newClient;
        }

        public Client? GetClientByUuid(Guid uuid)
        {
            return _clientRepository.GetByUuid(uuid);
        }

        public IEnumerable<Client> GetAllClients()
        {
            return _clientRepository.GetAll();
        }

        public void UpdateClient(Client client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Client must not be null.");
            }
            _clientRepository.Update(client);
        }

        public void DeleteClient(Guid clientId)
        {
            var client = _clientRepository.GetByUuid(clientId);
            if (client == null)
            {
                throw new ArgumentException("Client not found.", nameof(clientId));
            }
            if (client.Projects?.Count > 0)
            {
                throw new InvalidOperationException($"Kunden kan ikke slettes fordi den har {client.Projects.Count} projekt(er) forbundet.");
            }
            _clientRepository.Delete(clientId);
        }

    }
}