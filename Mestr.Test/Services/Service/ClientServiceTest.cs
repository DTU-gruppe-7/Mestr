using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Data.Repository;
using Mestr.Data.Interface;

namespace Mestr.Test.Services.Service
{
    /// <summary>
    /// Integration tests for ClientService - Testing client management logic (Simplified for async)
    /// </summary>
    public class ClientServiceTest : IAsyncLifetime
    {
        private readonly IClientService _sut;
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly List<Guid> _clientsToCleanup;
        private readonly List<Guid> _projectsToCleanup;

        public ClientServiceTest()
        {
            _clientRepository = new ClientRepository();
            _projectRepository = new ProjectRepository();
            _sut = new Mestr.Services.Service.ClientService(_clientRepository);
            _clientsToCleanup = new List<Guid>();
            _projectsToCleanup = new List<Guid>();
        }

        public ValueTask InitializeAsync()
        {
            return ValueTask.CompletedTask;
        }

        #region CreateClient Tests

        [Fact]
        public async Task CreateClient_WithValidBusinessData_ShouldCreateClient()
        {
            // Arrange
            var companyName = "Test Company ApS";
            var contactName = "John Doe";
            var email = "john@testcompany.dk";
            var phone = "12345678";
            var address = "Testvej 123";
            var postal = "1234";
            var city = "Testby";
            var cvr = "12345678";

            // Act
            var result = await _sut.CreateClientAsync(companyName, contactName, email, phone, address, postal, city, cvr);
            _clientsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(companyName, result.Name);
            Assert.Equal(contactName, result.ContactPerson);
            Assert.Equal(cvr, result.Cvr);
            Assert.True(result.IsBusinessClient());
        }

        [Fact]
        public async Task CreateClient_WithValidPrivateData_ShouldCreateClient()
        {
            // Arrange
            var name = "John Doe";
            var email = "john@private.dk";

            // Act
            var result = await _sut.CreateClientAsync(name, name, email, "12345678", "Privatvej 1", "1234", "Privatby", null);
            _clientsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Cvr);
            Assert.False(result.IsBusinessClient());
        }

        [Fact]
        public async Task CreateClient_WithNullContactName_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.CreateClientAsync("Company", null, "email@test.dk", "12345678", "Street", "1234", "City", "12345678"));
            Assert.Equal("contactPerson", exception.ParamName);
        }

        [Fact]
        public async Task CreateClient_WithNullEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.CreateClientAsync("Company", "Contact Person", null, "12345678", "Street", "1234", "City", "12345678"));
            Assert.Equal("email", exception.ParamName);
        }

        #endregion

        #region GetClientByUuid Tests

        [Fact]
        public async Task GetClientByUuid_WithExistingClient_ShouldReturnClient()
        {
            // Arrange
            var client = await _sut.CreateClientAsync("Company", "Contact", "email@test.dk", "12345678", "Street", "1234", "City", "12345678");
            _clientsToCleanup.Add(client.Uuid);

            // Act
            var result = await _sut.GetClientByUuidAsync(client.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(client.Uuid, result.Uuid);
        }

        [Fact]
        public async Task GetClientByUuid_WithNonExistentUuid_ShouldReturnNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act
            var result = await _sut.GetClientByUuidAsync(nonExistentUuid);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdateClient Tests

        [Fact]
        public async Task UpdateClient_WithValidClient_ShouldUpdateClient()
        {
            // Arrange
            var client = await _sut.CreateClientAsync("Original Company", "Original Contact", "original@test.dk", "12345678", "Original Street", "1234", "Original City", "12345678");
            _clientsToCleanup.Add(client.Uuid);

            // Act
            client.Name = "Updated Company";
            client.ContactPerson = "Updated Contact";
            await _sut.UpdateClientAsync(client);

            // Assert
            var updated = await _sut.GetClientByUuidAsync(client.Uuid);
            Assert.NotNull(updated);
            Assert.Equal("Updated Company", updated.Name);
            Assert.Equal("Updated Contact", updated.ContactPerson);
        }

        [Fact]
        public async Task UpdateClient_WithNullClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateClientAsync(null));
        }

        #endregion

        #region DeleteClient Tests

        [Fact]
        public async Task DeleteClient_WithClientWithoutProjects_ShouldDeleteClient()
        {
            // Arrange
            var client = await _sut.CreateClientAsync("Company", "Contact", "email@test.dk", "12345678", "Street", "1234", "City", "12345678");

            // Act
            await _sut.DeleteClientAsync(client.Uuid);

            // Assert
            var deleted = await _sut.GetClientByUuidAsync(client.Uuid);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteClient_WithNonExistentClient_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteClientAsync(nonExistentUuid));
            Assert.Equal("clientId", exception.ParamName);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public async Task IsBusinessClient_WithCVR_ShouldReturnTrue()
        {
            // Arrange
            var client = await _sut.CreateClientAsync("Company ApS", "Contact", "email@test.dk", "12345678", "Street", "1234", "City", "12345678");
            _clientsToCleanup.Add(client.Uuid);

            // Act
            var isBusinessClient = client.IsBusinessClient();

            // Assert
            Assert.True(isBusinessClient);
        }

        [Fact]
        public async Task IsBusinessClient_WithoutCVR_ShouldReturnFalse()
        {
            // Arrange
            var client = await _sut.CreateClientAsync("John Doe", "John Doe", "john@private.dk", "12345678", "Street", "1234", "City", null);
            _clientsToCleanup.Add(client.Uuid);

            // Act
            var isBusinessClient = client.IsBusinessClient();

            // Assert
            Assert.False(isBusinessClient);
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            foreach (var projectUuid in _projectsToCleanup)
            {
                try
                {
                    await _projectRepository.DeleteAsync(projectUuid);
                }
                catch { }
            }

            foreach (var clientUuid in _clientsToCleanup)
            {
                try
                {
                    await _clientRepository.DeleteAsync(clientUuid);
                }
                catch { }
            }
        }
    }
}
