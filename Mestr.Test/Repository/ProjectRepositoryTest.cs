using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Data.Interface;

namespace Mestr.Test.Repository
{
    public class ProjectRepositoryTest
    {
        private readonly IRepository<Project> _projectRepository;
        public ProjectRepositoryTest() {
            _projectRepository = new ProjectRepository();
        }

        [Fact]
        public void AddProject_ToRepository_Succeeds()
        {
            // Arrange
            var project = new Project
            (
                Guid.NewGuid(),
                "Test Project",
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(10)
            );
            // Act
            _projectRepository.Add(project);
            // Assert
            var retrievedProject = _projectRepository.GetByUuid(project.Uuid);
            Assert.NotNull(retrievedProject);
            Assert.Equal(project.Uuid, retrievedProject.Uuid);
        }

        [Fact]
        public void GetByUuid_ProjectExists_ReturnsProject()
        {
            // Arrange
            var project = new Project
            (
                Guid.NewGuid(),
                "Test Project",
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(10)
            );
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
            Project project = null;
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _projectRepository.Add(project));
        }

        [Fact]
        public void GetByUuid_emptyGuid_ThrowsArgumentException()
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
            var project1 = new Project
            (
                Guid.NewGuid(),
                "Test Project 1",
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is test project 1",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(10)
            );
            var project2 = new Project
            (
                Guid.NewGuid(),
                "Test Project 2",
                DateTime.Now,
                DateTime.Now.AddDays(2),
                "This is test project 2",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(20)
            );
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
            var project = new Project
            (
                Guid.NewGuid(),
                "Test Project",
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Ongoing,
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
            var project = new Project
            (
                Guid.NewGuid(),
                "Test Project",
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(10)
            );
            _projectRepository.Add(project);
            // Act
            project.Description = "Updated Description";
            _projectRepository.Update(project);
            // Assert
            var retrievedProject = _projectRepository.GetByUuid(project.Uuid);
            Assert.Equal("Updated Description", retrievedProject.Description);
        }

        [Fact]
        public void CleanupRepository_AfterTests()
        {
            // Arrange
            var project = new Project
            (
                Guid.NewGuid(),
                "Test Project",
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(10)
            );
            _projectRepository.Add(project);
            // Act
            _projectRepository.Delete(project.Uuid);
            // Assert
            var retrievedProject = _projectRepository.GetByUuid(project.Uuid);
            Assert.Null(retrievedProject);
        }

    }
}
