using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Mestr.Test.Services.Service
{
    /// <summary>
    /// Integration tests for ProjectService - Testing project management functionality
    /// </summary>
    public class ProjectServiceTest : IDisposable
    {
        private readonly Mestr.Services.Service.ProjectService _sut;
        private readonly ProjectRepository _projectRepository;
        private readonly ClientRepository _clientRepository;
        private readonly EarningRepository _earningRepository;
        private readonly ExpenseRepository _expenseRepository;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;
        private readonly List<Guid> _earningsToCleanup;
        private readonly List<Guid> _expensesToCleanup;
        private readonly string _testRunId;

        public ProjectServiceTest()
        {
            _sut = new Mestr.Services.Service.ProjectService();
            _projectRepository = new ProjectRepository();
            _clientRepository = new ClientRepository();
            _earningRepository = new EarningRepository();
            _expenseRepository = new ExpenseRepository();
            _projectsToCleanup = new List<Guid>();
            _clientsToCleanup = new List<Guid>();
            _earningsToCleanup = new List<Guid>();
            _expensesToCleanup = new List<Guid>();
            _testRunId = Guid.NewGuid().ToString().Substring(0, 8);
        }

        #region Helper Methods

        private Client CreateTestClient()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 12);
            var client = new Client(
                Guid.NewGuid(),
                $"TestCompany_{uniqueId}",
                $"TestPerson_{uniqueId}",
                $"test_{uniqueId}@company.dk",
                "12345678",
                "Test Street 1",
                "1234",
                "Test City",
                "12345678"
            );
            _clientRepository.Add(client);
            _clientsToCleanup.Add(client.Uuid);
            
            // Small delay to ensure database write
            System.Threading.Thread.Sleep(100);
            
            return client;
        }

        #endregion

        #region CreateProject Tests

        [Fact]
        public void CreateProject_WithValidData_ShouldCreateProject()
        {
            // Arrange
            var client = CreateTestClient();
            var projectName = $"TestProject_{_testRunId}";
            var description = "Test Description";

            // Act
            var result = _sut.CreateProject(projectName, client, description, null);
            _projectsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(projectName, result.Name);
            Assert.Equal(description, result.Description);
            Assert.Equal(client.Uuid, result.Client.Uuid);
            Assert.Equal(ProjectStatus.Planlagt, result.Status);
        }

        [Fact]
        public void CreateProject_WithNullName_ShouldThrowArgumentException()
        {
            // Arrange
            var client = CreateTestClient();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CreateProject(null, client, "Description", null));
            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void CreateProject_WithNullClient_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CreateProject("Project Name", null, "Description", null));
            Assert.Equal("client", exception.ParamName);
        }

        #endregion

        #region GetProjectByUuid Tests

        [Fact]
        public void GetProjectByUuid_WithExistingProject_ShouldReturnProject()
        {
            // Arrange
            var client = CreateTestClient();
            var project = _sut.CreateProject($"TestProject_{_testRunId}", client, "Description", null);
            _projectsToCleanup.Add(project.Uuid);

            // Act
            var result = _sut.GetProjectByUuid(project.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(project.Uuid, result.Uuid);
            Assert.Equal(project.Name, result.Name);
        }

        [Fact]
        public void GetProjectByUuid_WithNonExistentUuid_ShouldReturnNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act
            var result = _sut.GetProjectByUuid(nonExistentUuid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetProjectByUuid_WithEmptyGuid_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.GetProjectByUuid(Guid.Empty));
            Assert.Equal("uuid", exception.ParamName);
        }

        #endregion

        #region UpdateProject Tests

        [Fact]
        public void UpdateProject_WithValidProject_ShouldUpdateProject()
        {
            // Arrange
            var client = CreateTestClient();
            var project = _sut.CreateProject($"Original_{_testRunId}", client, "Original Desc", null);
            _projectsToCleanup.Add(project.Uuid);

            // Act
            project.Name = $"Updated_{_testRunId}";
            project.Description = "Updated Description";
            _sut.UpdateProject(project);
            
            System.Threading.Thread.Sleep(50);

            // Assert
            var updated = _sut.GetProjectByUuid(project.Uuid);
            Assert.Equal($"Updated_{_testRunId}", updated.Name);
            Assert.Equal("Updated Description", updated.Description);
        }

        [Fact]
        public void UpdateProject_WithNullProject_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _sut.UpdateProject(null));
        }

        #endregion

        #region CompleteProject Tests

        [Fact]
        public void CompleteProject_ShouldSetStatusAndEndDate()
        {
            // Arrange
            var client = CreateTestClient();
            var project = _sut.CreateProject($"ToComplete_{_testRunId}", client, "Desc", null);
            _projectsToCleanup.Add(project.Uuid);

            // Act
            _sut.CompleteProject(project.Uuid);
            System.Threading.Thread.Sleep(50);

            // Assert
            var completed = _sut.GetProjectByUuid(project.Uuid);
            Assert.Equal(ProjectStatus.Afsluttet, completed.Status);
            Assert.NotNull(completed.EndDate);
        }

        [Fact]
        public void CompleteProject_WithNonExistentProject_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.CompleteProject(nonExistentUuid));
            Assert.Equal("projectId", exception.ParamName);
        }

        #endregion

        #region DeleteProject Tests

        [Fact]
        public void DeleteProject_WithExistingProject_ShouldDeleteProject()
        {
            // Arrange
            var client = CreateTestClient();
            var project = _sut.CreateProject($"ToDelete_{_testRunId}", client, "Desc", null);

            // Act
            _sut.DeleteProject(project.Uuid);
            System.Threading.Thread.Sleep(50);

            // Assert
            var deleted = _sut.GetProjectByUuid(project.Uuid);
            Assert.Null(deleted);
        }

        [Fact]
        public void DeleteProject_WithEmptyGuid_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.DeleteProject(Guid.Empty));
            Assert.Equal("projectId", exception.ParamName);
        }

        #endregion

        public void Dispose()
        {
            // Cleanup earnings first
            foreach (var earningUuid in _earningsToCleanup)
            {
                try
                {
                    _earningRepository.Delete(earningUuid);
                }
                catch { }
            }

            // Cleanup expenses
            foreach (var expenseUuid in _expensesToCleanup)
            {
                try
                {
                    _expenseRepository.Delete(expenseUuid);
                }
                catch { }
            }

            // Small delay to ensure deletions complete
            System.Threading.Thread.Sleep(100);

            // Cleanup projects
            foreach (var projectUuid in _projectsToCleanup)
            {
                try
                {
                    _projectRepository.Delete(projectUuid);
                }
                catch { }
            }

            // Cleanup clients
            foreach (var clientUuid in _clientsToCleanup)
            {
                try
                {
                    _clientRepository.Delete(clientUuid);
                }
                catch { }
            }

            // Final delay to ensure all cleanups complete
            System.Threading.Thread.Sleep(100);
        }
    }
}
