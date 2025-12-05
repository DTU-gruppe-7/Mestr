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
    /// Integration tests for EarningService - Testing earning management logic (Simplified for async)
    /// </summary>
    public class EarningServiceTest : IAsyncLifetime
    {
        private readonly IEarningService _sut;
        private readonly IRepository<Earning> _earningRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly List<Guid> _earningsToCleanup;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;

        public EarningServiceTest()
        {
            _earningRepository = new EarningRepository();
            _projectRepository = new ProjectRepository();
            _clientRepository = new ClientRepository();
            _sut = new Mestr.Services.Service.EarningService(_earningRepository);
            _earningsToCleanup = new List<Guid>();
            _projectsToCleanup = new List<Guid>();
            _clientsToCleanup = new List<Guid>();
        }

        public ValueTask InitializeAsync()
        {
            return ValueTask.CompletedTask;
        }

        #region Helper Methods

        private async Task<Guid> CreateTestProjectAsync()
        {
            var client = Client.Create(
                Guid.NewGuid(),
                "Test Company",
                "Test Person",
                "test@company.dk",
                "12345678",
                "Test Street 1",
                "1234",
                "Test City",
                "12345678"
            );
            await _clientRepository.AddAsync(client);
            _clientsToCleanup.Add(client.Uuid);

            var project = new Project(
                Guid.NewGuid(),
                "Test Project",
                client,
                DateTime.Now,
                DateTime.Now,
                "Test Description",
                Core.Enum.ProjectStatus.Aktiv,
                null
            );
            await _projectRepository.AddAsync(project);
            _projectsToCleanup.Add(project.Uuid);

            return project.Uuid;
        }

        #endregion

        #region AddNewEarning Tests

        [Fact]
        public async Task AddNewEarning_WithValidData_ShouldCreateEarning()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();
            var description = "Konsulentydelse";
            var amount = 1000m;
            var date = DateTime.Now;

            // Act
            var result = await _sut.AddNewEarningAsync(projectUuid, description, amount, date);
            _earningsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(description, result.Description);
            Assert.Equal(amount, result.Amount);
            Assert.Equal(projectUuid, result.ProjectUuid);
            Assert.False(result.IsPaid);
        }

        [Fact]
        public async Task AddNewEarning_WithEmptyProjectUuid_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.AddNewEarningAsync(Guid.Empty, "Test", 1000m, DateTime.Now));
            Assert.Equal("projectUuid", exception.ParamName);
        }

        [Fact]
        public async Task AddNewEarning_WithNullDescription_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.AddNewEarningAsync(projectUuid, null, 1000m, DateTime.Now));
            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public async Task AddNewEarning_WithZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.AddNewEarningAsync(projectUuid, "Description", 0m, DateTime.Now));
            Assert.Equal("amount", exception.ParamName);
        }

        [Theory]
        [InlineData(0.01)]
        [InlineData(100.00)]
        [InlineData(1000.50)]
        public async Task AddNewEarning_WithValidAmounts_ShouldCreateEarning(decimal amount)
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();

            // Act
            var result = await _sut.AddNewEarningAsync(projectUuid, "Test Earning", amount, DateTime.Now);
            _earningsToCleanup.Add(result.Uuid);

            // Assert
            Assert.Equal(amount, result.Amount);
        }

        #endregion

        #region GetByUuid Tests

        [Fact]
        public async Task GetByUuid_WithExistingEarning_ShouldReturnEarning()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();
            var earning = await _sut.AddNewEarningAsync(projectUuid, "Test Earning", 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Act
            var result = await _sut.GetByUuidAsync(earning.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(earning.Uuid, result.Uuid);
        }

        [Fact]
        public async Task GetByUuid_WithEmptyGuid_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByUuidAsync(Guid.Empty));
            Assert.Equal("uuid", exception.ParamName);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidEarning_ShouldUpdateEarning()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();
            var earning = await _sut.AddNewEarningAsync(projectUuid, "Original Description", 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Act
            earning.Description = "Updated Description";
            earning.Amount = 2000m;
            var result = await _sut.UpdateAsync(earning);

            // Assert
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(2000m, result.Amount);
        }

        [Fact]
        public async Task Update_WithNullEarning_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAsync(null));
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithExistingEarning_ShouldDeleteEarning()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();
            var earning = await _sut.AddNewEarningAsync(projectUuid, "Test Earning", 1000m, DateTime.Now);

            // Act
            var result = await _sut.DeleteAsync(earning);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Delete_WithNullEarning_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteAsync(null));
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            foreach (var earningUuid in _earningsToCleanup)
            {
                try
                {
                    await _earningRepository.DeleteAsync(earningUuid);
                }
                catch { }
            }

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
