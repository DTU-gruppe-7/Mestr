using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Core.Constants;
using Mestr.Data.Repository;
using Mestr.Data.Interface;
using Mestr.Data.DbContext;

namespace Mestr.Test.Repository
{
    public class EarningRepositoryTest : IAsyncLifetime
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Earning> _earningRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly List<Guid> _earningsToCleanup;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;

        public EarningRepositoryTest()
        {
            _projectRepository = new ProjectRepository();
            _earningRepository = new EarningRepository();
            _clientRepository = new ClientRepository();
            _earningsToCleanup = new List<Guid>();
            _projectsToCleanup = new List<Guid>();
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

        private async Task<Project> CreateTestProjectAsync()
        {
            var client = Client.Create(Guid.NewGuid(), "Test Client", "John Doe", "test@something.com", "12345678", "123 Test St", "12345", "Test City", "88888888");
            await _clientRepository.AddAsync(client);
            _clientsToCleanup.Add(client.Uuid);

            var project = new Project(
                Guid.NewGuid(),
                "Test Project",
                client,
                DateTime.Now,
                DateTime.Now.AddMonths(1),
                "This is a testproject",
                ProjectStatus.Aktiv,
                DateTime.Now.AddMonths(10)
            );
            await _projectRepository.AddAsync(project);
            _projectsToCleanup.Add(project.Uuid);
            return project;
        }

        [Fact]
        public async Task AddToRepositoryTest()
        {
            // Arrange
            var testProject = await CreateTestProjectAsync();
            var testEarning = new Earning(
                Guid.NewGuid(),
                "Test Earning Description",
                1500.00m,
                DateTime.Now,
                false
            );
            testEarning.ProjectUuid = testProject.Uuid;
            _earningsToCleanup.Add(testEarning.Uuid);

            // Act
            await _earningRepository.AddAsync(testEarning);

            // Assert
            Earning? retrievedEarning = await _earningRepository.GetByUuidAsync(testEarning.Uuid);
            Assert.NotNull(retrievedEarning);
            Assert.Equal(testEarning.Uuid, retrievedEarning.Uuid);
            Assert.Equal(testEarning.ProjectUuid, retrievedEarning.ProjectUuid);
            Assert.Equal(testEarning.Description, retrievedEarning.Description);
            Assert.Equal(testEarning.Amount, retrievedEarning.Amount);
            Assert.Equal(testEarning.Date.ToString(AppConstants.DateTimeFormats.Standard), 
                        retrievedEarning.Date.ToString(AppConstants.DateTimeFormats.Standard));
            Assert.False(retrievedEarning.IsPaid);
            Assert.NotNull(retrievedEarning.Project);
            Assert.Equal(testProject.Uuid, retrievedEarning.Project.Uuid);
        }

        [Fact]
        public async Task GetByUuid_NonExistentEarning_ReturnsNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act
            Earning? retrievedEarning = await _earningRepository.GetByUuidAsync(nonExistentUuid);

            // Assert
            Assert.Null(retrievedEarning);
        }

        [Fact]
        public async Task Add_NullEarning_ThrowsArgumentNullException()
        {
            // Arrange
            Earning? nullEarning = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _earningRepository.AddAsync(nullEarning!));
        }

        [Fact]
        public async Task GetByUuid_EmptyGuid_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _earningRepository.GetByUuidAsync(emptyGuid));
        }

        [Fact]
        public async Task GetAll_EarningsExist_ReturnsAllEarnings()
        {
            // Arrange
            var testProject = await CreateTestProjectAsync();
            var testEarning1 = new Earning(
                Guid.NewGuid(),
                "Test Earning 1",
                1000.00m,
                DateTime.Now,
                false
            );
            testEarning1.ProjectUuid = testProject.Uuid;
            
            var testEarning2 = new Earning(
                Guid.NewGuid(),
                "Test Earning 2",
                2000.00m,
                DateTime.Now,
                true
            );
            testEarning2.ProjectUuid = testProject.Uuid;
            
            _earningsToCleanup.Add(testEarning1.Uuid);
            _earningsToCleanup.Add(testEarning2.Uuid);

            await _earningRepository.AddAsync(testEarning1);
            await _earningRepository.AddAsync(testEarning2);

            // Act
            var allEarnings = (await _earningRepository.GetAllAsync()).ToList();

            // Assert
            Assert.Contains(allEarnings, e => e.Uuid == testEarning1.Uuid);
            Assert.Contains(allEarnings, e => e.Uuid == testEarning2.Uuid);
        }

        [Fact]
        public async Task Delete_ExistentEarning_RemovesEarning()
        {
            // Arrange
            var testProject = await CreateTestProjectAsync();
            var testEarning = new Earning(
                Guid.NewGuid(),
                "Earning to be deleted",
                500.00m,
                DateTime.Now,
                false
            );
            testEarning.ProjectUuid = testProject.Uuid;
            await _earningRepository.AddAsync(testEarning);

            // Act
            await _earningRepository.DeleteAsync(testEarning.Uuid);

            // Assert
            Earning? retrievedEarning = await _earningRepository.GetByUuidAsync(testEarning.Uuid);
            Assert.Null(retrievedEarning);
        }

        [Fact]
        public async Task Update_ExistentEarning_UpdatesEarning()
        {
            // Arrange
            var testProject = await CreateTestProjectAsync();
            var testEarning = new Earning(
                Guid.NewGuid(),
                "Original Description",
                750.00m,
                DateTime.Now,
                false
            );
            testEarning.ProjectUuid = testProject.Uuid;
            _earningsToCleanup.Add(testEarning.Uuid);
            await _earningRepository.AddAsync(testEarning);

            // Act
            testEarning.Description = "Updated Description";
            testEarning.Amount = 1250.00m;
            testEarning.IsPaid = true;
            await _earningRepository.UpdateAsync(testEarning);

            // Assert
            Earning? retrievedEarning = await _earningRepository.GetByUuidAsync(testEarning.Uuid);
            Assert.NotNull(retrievedEarning);
            Assert.Equal("Updated Description", retrievedEarning.Description);
            Assert.Equal(1250.00m, retrievedEarning.Amount);
            Assert.True(retrievedEarning.IsPaid);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var earningUuid in _earningsToCleanup)
            {
                try
                {
                    await _earningRepository.DeleteAsync(earningUuid);
                }
                catch
                {
                    // Ignore if already deleted
                }
            }

            foreach (var projectUuid in _projectsToCleanup)
            {
                try
                {
                    await _projectRepository.DeleteAsync(projectUuid);
                }
                catch
                {
                    // Ignore if already deleted
                }
            }

            foreach (var clientUuid in _clientsToCleanup)
            {
                try
                {
                    await _clientRepository.DeleteAsync(clientUuid);
                }
                catch
                {
                    // Ignore if already deleted
                }
            }
        }
    }
}
