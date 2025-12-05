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
        public async Task AddClient_ToRepository_Succeeds()
        {
            // Arrange
            var client = Client.Create(Guid.NewGuid(), "Test Client", "John Doe", "test@something.com", "12345678", "123 Test St", "12345", "Test City", "88888888");
            _clientsToCleanup.Add(client.Uuid);

            // Act
            await _clientRepository.AddAsync(client);

            // Assert
            var retrievedClient = await _clientRepository.GetByUuidAsync(client.Uuid);
            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Uuid, retrievedClient.Uuid);
            Assert.Equal(client.Name, retrievedClient.Name);
            Assert.Equal(client.Email, retrievedClient.Email);
        }

        [Fact]
        public async Task GetByUuid_ClientExists_ReturnsClient()
        {
            // Arrange
            var client = Client.Create(Guid.NewGuid(), "Test Client", "Jane Doe", "jane@test.com", "87654321", "456 Test Ave", "54321", "Test Town", "99999999");
            _clientsToCleanup.Add(client.Uuid);
            await _clientRepository.AddAsync(client);

            // Act
            var retrievedClient = await _clientRepository.GetByUuidAsync(client.Uuid);

            // Assert
            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Uuid, retrievedClient.Uuid);
            Assert.Equal(client.ContactPerson, retrievedClient.ContactPerson);
        }

        [Fact]
        public async Task GetByUuid_NonExistentClient_ReturnsNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act
            var result = await _clientRepository.GetByUuidAsync(nonExistentUuid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddNullClient_ThrowsArgumentNullException()
        {
            // Arrange
            Client? client = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepository.AddAsync(client));
        }

        [Fact]
        public async Task GetByUuid_EmptyGuid_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepository.GetByUuidAsync(emptyGuid));
        }

        [Fact]
        public async Task GetAll_ReturnsAllClients()
        {
            // Arrange
            var client1 = Client.Create(Guid.NewGuid(), "Client 1", "Person 1", "client1@test.com", "11111111", "Address 1", "11111", "City 1", "12345678");
            var client2 = Client.Create(Guid.NewGuid(), "Client 2", "Person 2", "client2@test.com", "22222222", "Address 2", "22222", "City 2", "87654321");
            _clientsToCleanup.Add(client1.Uuid);
            _clientsToCleanup.Add(client2.Uuid);

            await _clientRepository.AddAsync(client1);
            await _clientRepository.AddAsync(client2);

            // Act
            var allClients = await _clientRepository.GetAllAsync();

            // Assert
            Assert.Contains(allClients, c => c.Uuid == client1.Uuid);
            Assert.Contains(allClients, c => c.Uuid == client2.Uuid);
        }

        [Fact]
        public async Task Update_ExistingClient_UpdatesClient()
        {
            // Arrange
            var client = Client.Create(Guid.NewGuid(), "Original Name", "Original Person", "original@test.com", "33333333", "Original Address", "33333", "Original City", "11111111");
            _clientsToCleanup.Add(client.Uuid);
            await _clientRepository.AddAsync(client);

            // Act
            client.Name = "Updated Name";
            client.Email = "updated@test.com";
            await _clientRepository.UpdateAsync(client);

            // Assert
            var retrievedClient = await _clientRepository.GetByUuidAsync(client.Uuid);
            Assert.Equal("Updated Name", retrievedClient.Name);
            Assert.Equal("updated@test.com", retrievedClient.Email);
        }

        [Fact]
        public async Task Delete_ExistingClient_RemovesClient()
        {
            // Arrange
            var client = Client.Create(Guid.NewGuid(), "To Be Deleted", "Delete Person", "delete@test.com", "44444444", "Delete Address", "44444", "Delete City", "22222222");
            _clientsToCleanup.Add(client.Uuid);
            await _clientRepository.AddAsync(client);

            // Act
            await _clientRepository.DeleteAsync(client.Uuid);

            // Assert
            var retrievedClient = await _clientRepository.GetByUuidAsync(client.Uuid);
            Assert.Null(retrievedClient);
        }

        [Fact]
        public void Create_WithInvalidEmail_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                Client.Create(Guid.NewGuid(), "Test", "Person", "invalid-email", "12345678", "Address", "12345", "City"));
        }

        [Fact]
        public void Create_WithInvalidPhoneNumber_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                Client.Create(Guid.NewGuid(), "Test", "Person", "test@example.com", "123", "Address", "12345", "City"));
        }

        [Fact]
        public void Create_WithEmptyContactPerson_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                Client.Create(Guid.NewGuid(), "Test", "", "test@example.com", "12345678", "Address", "12345", "City"));
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
