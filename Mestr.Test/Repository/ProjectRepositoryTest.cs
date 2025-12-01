using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Data.Interface;
using Mestr.Data.DbContext;

namespace Mestr.Test.Repository
{
    public class ProjectRepositoryTest : IAsyncLifetime
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;

        public ProjectRepositoryTest()
        {
            _projectRepository = new ProjectRepository();
            _clientRepository = new ClientRepository();
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

        private async Task<Client> CreateTestClientAsync()
        {
            var client = Client.Create(Guid.NewGuid(), "Test Client", "John Doe", "test@something.com", "12345678", "123 Test St", "12345", "Test City", "88888888");
            await _clientRepository.AddAsync(client);
            _clientsToCleanup.Add(client.Uuid);
            return client;
        }

        [Fact]
        public async Task AddProject_ToRepository_Succeeds()
        {
            // Arrange
            var client = await CreateTestClientAsync();
            var project = new Project(
                Guid.NewGuid(),
                "Test Project",
                client,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Aktiv,
                DateTime.Now.AddDays(10)
            );
            _projectsToCleanup.Add(project.Uuid);

            // Act
            await _projectRepository.AddAsync(project);

            // Assert
            var retrievedProject = await _projectRepository.GetByUuidAsync(project.Uuid);
            Assert.NotNull(retrievedProject);
            Assert.Equal(project.Uuid, retrievedProject.Uuid);
            Assert.NotNull(retrievedProject.Client);
            Assert.Equal(client.Uuid, retrievedProject.Client.Uuid);
        }

        [Fact]
        public async Task GetByUuid_ProjectExists_ReturnsProject()
        {
            // Arrange
            var client = await CreateTestClientAsync();
            var project = new Project(
                Guid.NewGuid(),
                "Test Project",
                client,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Aktiv,
                DateTime.Now.AddDays(10)
            );
            _projectsToCleanup.Add(project.Uuid);
            await _projectRepository.AddAsync(project);

            // Act
            var retrievedProject = await _projectRepository.GetByUuidAsync(project.Uuid);

            // Assert
            Assert.NotNull(retrievedProject);
            Assert.Equal(project.Uuid, retrievedProject.Uuid);
        }

        [Fact]
        public async Task AddNullProject_ThrowsArgumentNullException()
        {
            // Arrange
            Project? project = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _projectRepository.AddAsync(project));
        }

        [Fact]
        public async Task GetByUuid_EmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _projectRepository.GetByUuidAsync(emptyGuid));
        }

        [Fact]
        public async Task GetAll_ReturnsAllProjects()
        {
            // Arrange
            var client = await CreateTestClientAsync();
            var project1 = new Project(
                Guid.NewGuid(),
                "Test Project 1",
                client,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is test project 1",
                ProjectStatus.Aktiv,
                DateTime.Now.AddDays(10)
            );
            var project2 = new Project(
                Guid.NewGuid(),
                "Test Project 2",
                client,
                DateTime.Now,
                DateTime.Now.AddDays(2),
                "This is test project 2",
                ProjectStatus.Aktiv,
                DateTime.Now.AddDays(20)
            );
            _projectsToCleanup.Add(project1.Uuid);
            _projectsToCleanup.Add(project2.Uuid);

            await _projectRepository.AddAsync(project1);
            await _projectRepository.AddAsync(project2);

            // Act
            var allProjects = await _projectRepository.GetAllAsync();

            // Assert
            Assert.Contains(allProjects, p => p.Uuid == project1.Uuid);
            Assert.Contains(allProjects, p => p.Uuid == project2.Uuid);
        }

        [Fact]
        public async Task Delete_ExistingProject_RemovesProject()
        {
            // Arrange
            var client = await CreateTestClientAsync();
            var project = new Project(
                Guid.NewGuid(),
                "Test Project",
                client,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Aktiv,
                DateTime.Now.AddDays(10)
            );
            await _projectRepository.AddAsync(project);

            // Act
            await _projectRepository.DeleteAsync(project.Uuid);

            // Assert
            var retrievedProject = await _projectRepository.GetByUuidAsync(project.Uuid);
            Assert.Null(retrievedProject);
        }

        [Fact]
        public async Task Update_ExistingProject_UpdatesProject()
        {
            // Arrange
            var client = await CreateTestClientAsync();
            var project = new Project(
                Guid.NewGuid(),
                "Test Project",
                client,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Aktiv,
                DateTime.Now.AddDays(10)
            );
            _projectsToCleanup.Add(project.Uuid);
            await _projectRepository.AddAsync(project);

            // Act
            project.Description = "Updated Description";
            await _projectRepository.UpdateAsync(project);

            // Assert
            var retrievedProject = await _projectRepository.GetByUuidAsync(project.Uuid);
            Assert.Equal("Updated Description", retrievedProject.Description);
        }

        public async ValueTask DisposeAsync()
        {
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
