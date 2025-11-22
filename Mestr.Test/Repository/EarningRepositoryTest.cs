using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Data.Interface;

namespace Mestr.Test.Repository
{
    public class EarningRepositoryTest : IDisposable
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

        private Project CreateTestProject()
        {
            var client = new Client(Guid.NewGuid(), "Test Client", "John Doe", "test@something.com", "12345678", "123 Test St", "12345", "Test City", "88888888");
            _clientRepository.Add(client);
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
            _projectRepository.Add(project);
            _projectsToCleanup.Add(project.Uuid);
            return project;
        }

        [Fact]
        public void AddToRepositoryTest()
        {
            // Arrange
            var testProject = CreateTestProject();
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
            _earningRepository.Add(testEarning);

            // Assert
            Earning? retrievedEarning = _earningRepository.GetByUuid(testEarning.Uuid);
            Assert.NotNull(retrievedEarning);
            Assert.Equal(testEarning.Uuid, retrievedEarning.Uuid);
            Assert.Equal(testEarning.ProjectUuid, retrievedEarning.ProjectUuid);
            Assert.Equal(testEarning.Description, retrievedEarning.Description);
            Assert.Equal(testEarning.Amount, retrievedEarning.Amount);
            Assert.Equal(testEarning.Date.ToString("yyyy-MM-dd HH:mm:ss"), retrievedEarning.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.False(retrievedEarning.IsPaid);
            Assert.NotNull(retrievedEarning.Project);
            Assert.Equal(testProject.Uuid, retrievedEarning.Project.Uuid);
        }

        [Fact]
        public void GetByUuid_NonExistentEarning_ReturnsNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act
            Earning? retrievedEarning = _earningRepository.GetByUuid(nonExistentUuid);

            // Assert
            Assert.Null(retrievedEarning);
        }

        [Fact]
        public void Add_NullEarning_ThrowsArgumentNullException()
        {
            // Arrange
            Earning? nullEarning = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _earningRepository.Add(nullEarning!));
        }

        [Fact]
        public void GetByUuid_EmptyGuid_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _earningRepository.GetByUuid(emptyGuid));
        }

        [Fact]
        public void GetAll_EarningsExist_ReturnsAllEarnings()
        {
            // Arrange
            var testProject = CreateTestProject();
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

            _earningRepository.Add(testEarning1);
            _earningRepository.Add(testEarning2);

            // Act
            var allEarnings = _earningRepository.GetAll().ToList();

            // Assert
            Assert.Contains(allEarnings, e => e.Uuid == testEarning1.Uuid);
            Assert.Contains(allEarnings, e => e.Uuid == testEarning2.Uuid);
        }

        [Fact]
        public void Delete_ExistentEarning_RemovesEarning()
        {
            // Arrange
            var testProject = CreateTestProject();
            var testEarning = new Earning(
                Guid.NewGuid(),
                "Earning to be deleted",
                500.00m,
                DateTime.Now,
                false
            );
            testEarning.ProjectUuid = testProject.Uuid;
            _earningRepository.Add(testEarning);

            // Act
            _earningRepository.Delete(testEarning.Uuid);

            // Assert
            Earning? retrievedEarning = _earningRepository.GetByUuid(testEarning.Uuid);
            Assert.Null(retrievedEarning);
        }

        [Fact]
        public void Update_ExistentEarning_UpdatesEarning()
        {
            // Arrange
            var testProject = CreateTestProject();
            var testEarning = new Earning(
                Guid.NewGuid(),
                "Original Description",
                750.00m,
                DateTime.Now,
                false
            );
            testEarning.ProjectUuid = testProject.Uuid;
            _earningsToCleanup.Add(testEarning.Uuid);
            _earningRepository.Add(testEarning);

            // Act
            testEarning.Description = "Updated Description";
            testEarning.Amount = 1250.00m;
            testEarning.IsPaid = true;
            _earningRepository.Update(testEarning);

            // Assert
            Earning? retrievedEarning = _earningRepository.GetByUuid(testEarning.Uuid);
            Assert.NotNull(retrievedEarning);
            Assert.Equal("Updated Description", retrievedEarning.Description);
            Assert.Equal(1250.00m, retrievedEarning.Amount);
            Assert.True(retrievedEarning.IsPaid);
        }

        public void Dispose()
        {
            // Cleanup in correct order due to foreign keys
            foreach (var earningUuid in _earningsToCleanup)
            {
                try
                {
                    _earningRepository.Delete(earningUuid);
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
                    _projectRepository.Delete(projectUuid);
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
                    _clientRepository.Delete(clientUuid);
                }
                catch
                {
                    // Ignore if already deleted
                }
            }
        }
    }
}
