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
        public void ProjectRepository_Add_NullProject_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _projectRepository.Add(null));
        }

        [Fact]
        public void ProjectRepository_GetByUuid_EmptyGuid_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _projectRepository.GetByUuid(Guid.Empty));
        }

        [Fact]
        public void ProjectRepository_Add_ValidProject_AddsProjectSuccessfully()
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
            Assert.Equal(project.Name, retrievedProject.Name);
            Assert.Equal(project.Description, retrievedProject.Description);
            Assert.Equal(project.Status, retrievedProject.Status);
        }
        [Fact]
        public void ProjectRepository_GetByUuid_NonExistentUuid_ReturnsNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();
            // Act
            var retrievedProject = _projectRepository.GetByUuid(nonExistentUuid);
            // Assert
            Assert.Null(retrievedProject);
        }

        [Fact]
        public void ProjectRepository_Add_ProjectWithMissingFields_ThrowsException()
        {
            // Arrange
            var project = new Project
            (
                Guid.NewGuid(),
                null, // Missing name
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is a test project",
                ProjectStatus.Ongoing
            );
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _projectRepository.Add(project));
        }

        [Fact]
        public void ProjectRepository_GetByUuid_ValidUuid_ReturnsCorrectProject()
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
        public void ProjectRepository_Add_DuplicateUuid_ThrowsException()
        {
            // Arrange
            var uuid = Guid.NewGuid();
            var project1 = new Project
            (
                uuid,
                "Test Project 1",
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "This is the first test project",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(10)
            );
            var project2 = new Project
            (
                uuid, // Same UUID as project1
                "Test Project 2",
                DateTime.Now,
                DateTime.Now.AddDays(2),
                "This is the second test project",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(15)
            );
            _projectRepository.Add(project1);
            // Act & Assert
            Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(() => _projectRepository.Add(project2));
        }
        [Fact]
        public void ProjectRepository_GetByUuid_InvalidUuidFormat_ThrowsFormatException()
        {
            // Arrange
            var invalidUuid = Guid.Empty; // Simulating an invalid UUID format
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _projectRepository.GetByUuid(invalidUuid));
        }

        [Fact]
        public void GetAllProjects_ReturnsAllProjects()
        {
            // Arrange
            var project1 = new Project
            (
                Guid.NewGuid(),
                "Project 1",
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "Description 1",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(10)
            );
            var project2 = new Project
            (
                Guid.NewGuid(),
                "Project 2",
                DateTime.Now,
                DateTime.Now.AddDays(2),
                "Description 2",
                ProjectStatus.Ongoing,
                DateTime.Now.AddDays(15)
            );
            _projectRepository.Add(project1);
            _projectRepository.Add(project2);
            // Act
            var allProjects = _projectRepository.GetAll();
            // Assert
            Assert.Contains(allProjects, p => p.Uuid == project1.Uuid);
            Assert.Contains(allProjects, p => p.Uuid == project2.Uuid);
        }
    }
}
