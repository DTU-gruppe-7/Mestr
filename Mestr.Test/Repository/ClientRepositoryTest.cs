using Xunit;
using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Data.Repository;
using Mestr.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mestr.Test.Repository
{
    public class ClientRepositoryTest : IAsyncLifetime
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly List<Guid> _clientsToCleanup;

        public ClientRepositoryTest()
        {
            _clientRepository = new ClientRepository();
            _clientsToCleanup = new List<Guid>();
        }

        public ValueTask InitializeAsync()
        {
            using (var context = new dbContext())
            {
                context.Database.EnsureCreated();
            }
            return ValueTask.CompletedTask;
        }

        [Fact]
        public void AddClient_ToRepository_Succeeds()
        {
            // Arrange
            var client = new Client(Guid.NewGuid(), "Test Client", "John Doe", "test@something.com", "12345678", "123 Test St", "12345", "Test City", "88888888");
            _clientsToCleanup.Add(client.Uuid);

            // Act
            _clientRepository.AddAsync(client).Wait();

            // Assert
            var retrievedClient = _clientRepository.GetByUuidAsync(client.Uuid).Result;
            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Uuid, retrievedClient.Uuid);
            Assert.Equal(client.Name, retrievedClient.Name);
            Assert.Equal(client.Email, retrievedClient.Email);
        }

        [Fact]
        public void GetByUuid_ClientExists_ReturnsClient()
        {
            // Arrange
            var client = new Client(Guid.NewGuid(), "Test Client", "Jane Doe", "jane@test.com", "87654321", "456 Test Ave", "54321", "Test Town", "99999999");
            _clientsToCleanup.Add(client.Uuid);
            _clientRepository.AddAsync(client).Wait();

            // Act
            var retrievedClient = _clientRepository.GetByUuidAsync(client.Uuid).Result;

            // Assert
            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Uuid, retrievedClient.Uuid);
            Assert.Equal(client.ContactPerson, retrievedClient.ContactPerson);
        }

        [Fact]
        public void GetByUuid_NonExistentClient_ReturnsNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act
            var result = _clientRepository.GetByUuidAsync(nonExistentUuid).Result;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void AddNullClient_ThrowsArgumentNullException()
        {
            // Arrange
            Client? client = null;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepository.AddAsync(client)).Wait();
        }

        [Fact]
        public void GetByUuid_EmptyGuid_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepository.GetByUuidAsync(emptyGuid)).Wait();
        }

        [Fact]
        public void GetAll_ReturnsAllClients()
        {
            // Arrange
            var client1 = new Client(Guid.NewGuid(), "Client 1", "Person 1", "client1@test.com", "11111111", "Address 1", "11111", "City 1", "12345678");
            var client2 = new Client(Guid.NewGuid(), "Client 2", "Person 2", "client2@test.com", "22222222", "Address 2", "22222", "City 2", "87654321");
            _clientsToCleanup.Add(client1.Uuid);
            _clientsToCleanup.Add(client2.Uuid);

            _clientRepository.AddAsync(client1).Wait();
            _clientRepository.AddAsync(client2).Wait();

            // Act
            var allClients = _clientRepository.GetAllAsync().Result;

            // Assert
            Assert.Contains(allClients, c => c.Uuid == client1.Uuid);
            Assert.Contains(allClients, c => c.Uuid == client2.Uuid);
        }

        [Fact]
        public void Update_ExistingClient_UpdatesClient()
        {
            // Arrange
            var client = new Client(Guid.NewGuid(), "Original Name", "Original Person", "original@test.com", "33333333", "Original Address", "33333", "Original City", "11111111");
            _clientsToCleanup.Add(client.Uuid);
            _clientRepository.AddAsync(client).Wait();

            // Act
            client.Name = "Updated Name";
            client.Email = "updated@test.com";
            _clientRepository.UpdateAsync(client).Wait();

            // Assert
            var retrievedClient = _clientRepository.GetByUuidAsync(client.Uuid).Result;
            Assert.Equal("Updated Name", retrievedClient.Name);
            Assert.Equal("updated@test.com", retrievedClient.Email);
        }

        [Fact]
        public void Delete_ExistingClient_RemovesClient()
        {
            // Arrange
            var client = new Client(Guid.NewGuid(), "To Be Deleted", "Delete Person", "delete@test.com", "44444444", "Delete Address", "44444", "Delete City", "22222222");
            _clientsToCleanup.Add(client.Uuid);
            _clientRepository.AddAsync(client).Wait();

            // Act
            _clientRepository.DeleteAsync(client.Uuid).Wait();

            // Assert
            var retrievedClient = _clientRepository.GetByUuidAsync(client.Uuid).Result;
            Assert.Null(retrievedClient);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var clientUuid in _clientsToCleanup)
            {
                try
                {
                    var client = await _clientRepository.GetByUuidAsync(clientUuid);
                    if (client != null)
                    {
                        await _clientRepository.DeleteAsync(clientUuid);
                    }
                }
                catch
                {
                    // Ignore if already deleted or other issues
                }
            }
        }
    }
}
