using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Data.Interface;

namespace Mestr.Test.Repository
{
    public class ProjectRepositoryTest : IDisposable
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

        private Client CreateTestClient()
        {
            var client = new Client(Guid.NewGuid(), "Test Client", "John Doe", "test@something.com", "12345678", "123 Test St", "12345", "Test City", "88888888");
            _clientRepository.Add(client);
            _clientsToCleanup.Add(client.Uuid);
            return client;
        }

        [Fact]
        public void AddProject_ToRepository_Succeeds()
        {
            // Arrange
            var client = CreateTestClient();
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
            _projectRepository.Add(project);

            // Assert
            var retrievedProject = _projectRepository.GetByUuid(project.Uuid);
            Assert.NotNull(retrievedProject);
            Assert.Equal(project.Uuid, retrievedProject.Uuid);
            Assert.NotNull(retrievedProject.Client);
            Assert.Equal(client.Uuid, retrievedProject.Client.Uuid);
        }

        [Fact]
        public void GetByUuid_ProjectExists_ReturnsProject()
        {
            // Arrange
            var client = CreateTestClient();
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
            _projectRepository.Add(project);

            // Act
            var retrievedProject = _projectRepository.GetByUuid(project.Uuid);

            // Assert
            Assert.NotNull(retrievedProject);
            Assert.Equal(project.Uuid, retrievedProject.Uuid);
        }

        [Fact]
        public void AddNullProject_ThrowsArgumentNullException()
        {
            // Arrange
            Project? project = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _projectRepository.Add(project));
        }

        [Fact]
        public void GetByUuid_EmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _projectRepository.GetByUuid(emptyGuid));
        }

        [Fact]
        public void GetAll_ReturnsAllProjects()
        {
            // Arrange
            var client = CreateTestClient();
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

            _projectRepository.Add(project1);
            _projectRepository.Add(project2);

            // Act
            var allProjects = _projectRepository.GetAll();

            // Assert
            Assert.Contains(allProjects, p => p.Uuid == project1.Uuid);
            Assert.Contains(allProjects, p => p.Uuid == project2.Uuid);
        }

        [Fact]
        public void Delete_ExistingProject_RemovesProject()
        {
            // Arrange
            var client = CreateTestClient();
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
            _projectRepository.Add(project);

            // Act
            _projectRepository.Delete(project.Uuid);

            // Assert
            var retrievedProject = _projectRepository.GetByUuid(project.Uuid);
            Assert.Null(retrievedProject);
        }

        [Fact]
        public void Update_ExistingProject_UpdatesProject()
        {
            // Arrange
            var client = CreateTestClient();
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
            _projectRepository.Add(project);

            // Act
            project.Description = "Updated Description";
            _projectRepository.Update(project);

            // Assert
            var retrievedProject = _projectRepository.GetByUuid(project.Uuid);
            Assert.Equal("Updated Description", retrievedProject.Description);
        }

        public void Dispose()
        {
            // Cleanup projects first (due to foreign key constraint)
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

            // Then cleanup clients
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
