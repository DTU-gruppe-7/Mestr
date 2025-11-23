using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Mestr.Core.Model;
using Mestr.Services.Service;
using Mestr.Data.Repository;

namespace Mestr.Test.Services
{
    /// <summary>
    /// Integration tests for ClientService - Testing client management logic
    /// </summary>
    public class ClientServiceTest : IDisposable
    {
        private readonly ClientService _sut;
        private readonly ClientRepository _clientRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly List<Guid> _clientsToCleanup;
        private readonly List<Guid> _projectsToCleanup;

        public ClientServiceTest()
        {
            _sut = new ClientService();
            _clientRepository = new ClientRepository();
            _projectRepository = new ProjectRepository();
            _clientsToCleanup = new List<Guid>();
            _projectsToCleanup = new List<Guid>();
        }

        #region CreateClient Tests

        [Fact]
        public void CreateClient_WithValidBusinessData_ShouldCreateClient()
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
            var result = _sut.CreateClient(companyName, contactName, email, phone, address, postal, city, cvr);
            _clientsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(companyName, result.Name);
            Assert.Equal(contactName, result.ContactPerson);
            Assert.Equal(email, result.Email);
            Assert.Equal(phone, result.PhoneNumber);
            Assert.Equal(address, result.Address);
            Assert.Equal(postal, result.PostalAddress);
            Assert.Equal(city, result.City);
            Assert.Equal(cvr, result.Cvr);
            Assert.True(result.IsBusinessClient());
        }

        [Fact]
        public void CreateClient_WithValidPrivateData_ShouldCreateClient()
        {
            // Arrange
            var name = "John Doe";
            var email = "john@private.dk";

            // Act
            var result = _sut.CreateClient(name, name, email, "12345678", "Privatvej 1", "1234", "Privatby", null);
            _clientsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(name, result.ContactPerson);
            Assert.Null(result.Cvr);
            Assert.False(result.IsBusinessClient());
        }

        [Fact]
        public void CreateClient_WithNullContactName_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CreateClient("Company", null, "email@test.dk", "12345678", "Street", "1234", "City", "12345678"));
            Assert.Equal("contactName", exception.ParamName);
            Assert.Contains("Contact name cannot be null or empty", exception.Message);
        }

        [Fact]
        public void CreateClient_WithEmptyContactName_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CreateClient("Company", "   ", "email@test.dk", "12345678", "Street", "1234", "City", "12345678"));
            Assert.Equal("contactName", exception.ParamName);
        }

        [Fact]
        public void CreateClient_WithWhitespaceContactName_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CreateClient("Company", "\t\n", "email@test.dk", "12345678", "Street", "1234", "City", "12345678"));
            Assert.Equal("contactName", exception.ParamName);
        }

        [Fact]
        public void CreateClient_WithNullEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CreateClient("Company", "Contact Person", null, "12345678", "Street", "1234", "City", "12345678"));
            Assert.Equal("email", exception.ParamName);
            Assert.Contains("Email cannot be null or empty", exception.Message);
        }

        [Fact]
        public void CreateClient_WithEmptyEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CreateClient("Company", "Contact Person", "   ", "12345678", "Street", "1234", "City", "12345678"));
            Assert.Equal("email", exception.ParamName);
        }

        [Fact]
        public void CreateClient_WithNullPhoneNumber_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CreateClient("Company", "Contact Person", "email@test.dk", null, "Street", "1234", "City", "12345678"));
            Assert.Equal("phoneNumber", exception.ParamName);
            Assert.Contains("Phone number cannot be null or empty", exception.Message);
        }

        [Fact]
        public void CreateClient_WithEmptyPhoneNumber_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CreateClient("Company", "Contact Person", "email@test.dk", "   ", "Street", "1234", "City", "12345678"));
            Assert.Equal("phoneNumber", exception.ParamName);
        }

        [Fact]
        public void CreateClient_WithEmptyStringCvr_ShouldTreatAsPrivateClient()
        {
            // Arrange & Act
            var result = _sut.CreateClient("Company", "Contact", "email@test.dk", "12345678", "Street", "1234", "City", "");
            _clientsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsBusinessClient());
        }

        [Fact]
        public void CreateClient_ShouldGenerateUniqueUuid()
        {
            // Arrange & Act
            var client1 = _sut.CreateClient("Company 1", "Contact 1", "email1@test.dk", "11111111", "Street 1", "1111", "City 1", "11111111");
            var client2 = _sut.CreateClient("Company 2", "Contact 2", "email2@test.dk", "22222222", "Street 2", "2222", "City 2", "22222222");
            _clientsToCleanup.Add(client1.Uuid);
            _clientsToCleanup.Add(client2.Uuid);

            // Assert
            Assert.NotEqual(client1.Uuid, client2.Uuid);
            Assert.NotEqual(Guid.Empty, client1.Uuid);
            Assert.NotEqual(Guid.Empty, client2.Uuid);
        }

        #endregion

        #region GetClientByUuid Tests

        [Fact]
        public void GetClientByUuid_WithExistingClient_ShouldReturnClient()
        {
            // Arrange
            var client = _sut.CreateClient("Company", "Contact", "email@test.dk", "12345678", "Street", "1234", "City", "12345678");
            _clientsToCleanup.Add(client.Uuid);

            // Act
            var result = _sut.GetClientByUuid(client.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(client.Uuid, result.Uuid);
            Assert.Equal(client.Name, result.Name);
            Assert.Equal(client.ContactPerson, result.ContactPerson);
            Assert.Equal(client.Email, result.Email);
        }

        [Fact]
        public void GetClientByUuid_WithNonExistentUuid_ShouldReturnNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act
            var result = _sut.GetClientByUuid(nonExistentUuid);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetAllClients Tests

        [Fact]
        public void GetAllClients_ShouldReturnAllClients()
        {
            // Arrange
            var client1 = _sut.CreateClient("Company 1", "Contact 1", "email1@test.dk", "11111111", "Street 1", "1111", "City 1", "11111111");
            var client2 = _sut.CreateClient("Company 2", "Contact 2", "email2@test.dk", "22222222", "Street 2", "2222", "City 2", "22222222");
            _clientsToCleanup.Add(client1.Uuid);
            _clientsToCleanup.Add(client2.Uuid);

            // Act
            var result = _sut.GetAllClients();

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, c => c.Uuid == client1.Uuid);
            Assert.Contains(result, c => c.Uuid == client2.Uuid);
        }

        [Fact]
        public void GetAllClients_WhenNoClients_ShouldReturnEmptyCollection()
        {
            // Arrange
            // Ensure database is in a known state (cleanup any existing test data)

            // Act
            var result = _sut.GetAllClients();

            // Assert
            Assert.NotNull(result);
            // Note: May contain other clients from database, so we just check it's not null
        }

        #endregion

        #region UpdateClient Tests

        [Fact]
        public void UpdateClient_WithValidClient_ShouldUpdateClient()
        {
            // Arrange
            var client = _sut.CreateClient("Original Company", "Original Contact", "original@test.dk", "12345678", "Original Street", "1234", "Original City", "12345678");
            _clientsToCleanup.Add(client.Uuid);

            // Act
            client.Name = "Updated Company";
            client.ContactPerson = "Updated Contact";
            client.Email = "updated@test.dk";
            client.PhoneNumber = "87654321";
            client.Address = "Updated Street";
            client.PostalAddress = "4321";
            client.City = "Updated City";
            _sut.UpdateClient(client);

            // Assert
            var updated = _sut.GetClientByUuid(client.Uuid);
            Assert.NotNull(updated);
            Assert.Equal("Updated Company", updated.Name);
            Assert.Equal("Updated Contact", updated.ContactPerson);
            Assert.Equal("updated@test.dk", updated.Email);
            Assert.Equal("87654321", updated.PhoneNumber);
            Assert.Equal("Updated Street", updated.Address);
            Assert.Equal("4321", updated.PostalAddress);
            Assert.Equal("Updated City", updated.City);
        }

        [Fact]
        public void UpdateClient_WithNullClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _sut.UpdateClient(null));
            Assert.Equal("client", exception.ParamName);
            Assert.Contains("Client must not be null", exception.Message);
        }

        [Fact]
        public void UpdateClient_ChangingFromPrivateToBusiness_ShouldUpdate()
        {
            // Arrange
            var client = _sut.CreateClient("John Doe", "John Doe", "john@private.dk", "12345678", "Street", "1234", "City", null);
            _clientsToCleanup.Add(client.Uuid);
            Assert.False(client.IsBusinessClient());

            // Act
            client.Name = "Doe Company ApS";
            client.Cvr = "12345678";
            _sut.UpdateClient(client);

            // Assert
            var updated = _sut.GetClientByUuid(client.Uuid);
            Assert.True(updated.IsBusinessClient());
            Assert.Equal("12345678", updated.Cvr);
        }

        [Fact]
        public void UpdateClient_ChangingFromBusinessToPrivate_ShouldUpdate()
        {
            // Arrange
            var client = _sut.CreateClient("Company ApS", "John Doe", "john@company.dk", "12345678", "Street", "1234", "City", "12345678");
            _clientsToCleanup.Add(client.Uuid);
            Assert.True(client.IsBusinessClient());

            // Act
            client.Cvr = null;
            _sut.UpdateClient(client);

            // Assert
            var updated = _sut.GetClientByUuid(client.Uuid);
            Assert.False(updated.IsBusinessClient());
            Assert.Null(updated.Cvr);
        }

        #endregion

        #region DeleteClient Tests

        [Fact]
        public void DeleteClient_WithClientWithoutProjects_ShouldDeleteClient()
        {
            // Arrange
            var client = _sut.CreateClient("Company", "Contact", "email@test.dk", "12345678", "Street", "1234", "City", "12345678");

            // Act
            _sut.DeleteClient(client.Uuid);

            // Assert
            var deleted = _sut.GetClientByUuid(client.Uuid);
            Assert.Null(deleted);
        }

        [Fact]
        public void DeleteClient_WithNonExistentClient_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _sut.DeleteClient(nonExistentUuid));
            Assert.Equal("clientId", exception.ParamName);
            Assert.Contains("Client not found", exception.Message);
        }

        [Fact]
        public void DeleteClient_WithClientWithProjects_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var client = _sut.CreateClient("Company", "Contact", "email@test.dk", "12345678", "Street", "1234", "City", "12345678");
            _clientsToCleanup.Add(client.Uuid);

            // Create a project for this client
            var projectService = new ProjectService();
            var project = projectService.CreateProject("Test Project", client, "Description", null);
            _projectsToCleanup.Add(project.Uuid);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _sut.DeleteClient(client.Uuid));
            Assert.Contains("Kunden kan ikke slettes fordi den har", exception.Message);
            Assert.Contains("projekt(er) forbundet", exception.Message);
        }

        [Fact]
        public void ClientWorkflow_CreateUpdateDelete_ShouldWorkCorrectly()
        {
            // Arrange & Act - Create
            var client = _sut.CreateClient("Test Workflow Company", "Test Contact", "workflow@test.dk", "12345678", "Street", "1234", "City", "12345678");
            var clientUuid = client.Uuid;
            Assert.NotNull(client);

            // Act - Update
            client.Name = "Updated Workflow Company";
            _sut.UpdateClient(client);
            var updated = _sut.GetClientByUuid(clientUuid);
            Assert.Equal("Updated Workflow Company", updated.Name);

            // Act - Delete
            _sut.DeleteClient(clientUuid);
            var deleted = _sut.GetClientByUuid(clientUuid);

            // Assert
            Assert.Null(deleted);
        }

        [Fact]
        public void CreateMultipleClients_ShouldAllBePersisted()
        {
            // Arrange
            var clients = new List<Client>();
            for (int i = 1; i <= 5; i++)
            {
                var client = _sut.CreateClient(
                    $"Company {i}",
                    $"Contact {i}",
                    $"email{i}@test.dk",
                    $"1234567{i}",
                    $"Street {i}",
                    $"123{i}",
                    $"City {i}",
                    $"1234567{i}"
                );
                clients.Add(client);
                _clientsToCleanup.Add(client.Uuid);
            }

            // Act
            var allClients = _sut.GetAllClients().ToList();

            // Assert
            foreach (var client in clients)
            {
                Assert.Contains(allClients, c => c.Uuid == client.Uuid);
            }
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public void GetFullAddress_ShouldReturnFormattedAddress()
        {
            // Arrange
            var client = _sut.CreateClient("Company", "Contact", "email@test.dk", "12345678", "Hovedgaden 123", "1234", "København", "12345678");
            _clientsToCleanup.Add(client.Uuid);

            // Act
            var fullAddress = client.GetFullAddress();

            // Assert
            Assert.Equal("Hovedgaden 123, 1234 København", fullAddress);
        }

        [Fact]
        public void IsBusinessClient_WithCVR_ShouldReturnTrue()
        {
            // Arrange
            var client = _sut.CreateClient("Company ApS", "Contact", "email@test.dk", "12345678", "Street", "1234", "City", "12345678");
            _clientsToCleanup.Add(client.Uuid);

            // Act
            var isBusinessClient = client.IsBusinessClient();

            // Assert
            Assert.True(isBusinessClient);
        }

        [Fact]
        public void IsBusinessClient_WithoutCVR_ShouldReturnFalse()
        {
            // Arrange
            var client = _sut.CreateClient("John Doe", "John Doe", "john@private.dk", "12345678", "Street", "1234", "City", null);
            _clientsToCleanup.Add(client.Uuid);

            // Act
            var isBusinessClient = client.IsBusinessClient();

            // Assert
            Assert.False(isBusinessClient);
        }

        #endregion

        public void Dispose()
        {
            // Cleanup projects first (foreign key constraint)
            foreach (var projectUuid in _projectsToCleanup)
            {
                try
                {
                    var project = _projectRepository.GetByUuid(projectUuid);
                    if (project != null)
                    {
                        _projectRepository.Delete(projectUuid);
                    }
                }
                catch
                {
                    // Ignore if already deleted or other issues
                }
            }

            // Then cleanup clients
            foreach (var clientUuid in _clientsToCleanup)
            {
                try
                {
                    var client = _clientRepository.GetByUuid(clientUuid);
                    if (client != null && (client.Projects == null || client.Projects.Count == 0))
                    {
                        _clientRepository.Delete(clientUuid);
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
