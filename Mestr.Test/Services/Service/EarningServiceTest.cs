using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Mestr.Core.Model;
using Mestr.Services.Service;
using Mestr.Data.Repository;

namespace Mestr.Test.Services.Service
{
    /// <summary>
    /// Integration tests for EarningService - Testing earning management logic
    /// </summary>
    public class EarningServiceTest : IDisposable
    {
        private readonly EarningService _sut;
        private readonly EarningRepository _earningRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly ClientRepository _clientRepository;
        private readonly List<Guid> _earningsToCleanup;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;

        public EarningServiceTest()
        {
            _sut = new EarningService();
            _earningRepository = new EarningRepository();
            _projectRepository = new ProjectRepository();
            _clientRepository = new ClientRepository();
            _earningsToCleanup = new List<Guid>();
            _projectsToCleanup = new List<Guid>();
            _clientsToCleanup = new List<Guid>();
        }

        #region Helper Methods

        private Guid CreateTestProject()
        {
            var client = new Client(
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
            _clientRepository.Add(client);
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
            _projectRepository.Add(project);
            _projectsToCleanup.Add(project.Uuid);

            return project.Uuid;
        }

        #endregion

        #region AddNewEarning Tests

        [Fact]
        public void AddNewEarning_WithValidData_ShouldCreateEarning()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var description = "Konsulentydelse";
            var amount = 1000m;
            var date = DateTime.Now;

            // Act
            var result = _sut.AddNewEarning(projectUuid, description, amount, date);
            _earningsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(description, result.Description);
            Assert.Equal(amount, result.Amount);
            Assert.Equal(date.Date, result.Date.Date);
            Assert.Equal(projectUuid, result.ProjectUuid);
            Assert.False(result.IsPaid);
        }

        [Fact]
        public void AddNewEarning_WithEmptyProjectUuid_ShouldThrowArgumentException()
        {
            // Arrange
            var description = "Test Earning";
            var amount = 1000m;
            var date = DateTime.Now;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewEarning(Guid.Empty, description, amount, date));
            Assert.Equal("projectUuid", exception.ParamName);
            Assert.Contains("Project UUID cannot be empty", exception.Message);
        }

        [Fact]
        public void AddNewEarning_WithNullDescription_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewEarning(projectUuid, null, 1000m, DateTime.Now));
            Assert.Equal("description", exception.ParamName);
            Assert.Contains("Description cannot be empty", exception.Message);
        }

        [Fact]
        public void AddNewEarning_WithEmptyDescription_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewEarning(projectUuid, "   ", 1000m, DateTime.Now));
            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public void AddNewEarning_WithWhitespaceDescription_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewEarning(projectUuid, "\t\n", 1000m, DateTime.Now));
            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public void AddNewEarning_WithZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewEarning(projectUuid, "Description", 0m, DateTime.Now));
            Assert.Equal("amount", exception.ParamName);
            Assert.Contains("Amount must be greater than zero", exception.Message);
        }

        [Fact]
        public void AddNewEarning_WithNegativeAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewEarning(projectUuid, "Description", -100m, DateTime.Now));
            Assert.Equal("amount", exception.ParamName);
        }

        [Theory]
        [InlineData(0.01)]
        [InlineData(1.00)]
        [InlineData(100.00)]
        [InlineData(1000.50)]
        [InlineData(10000.99)]
        [InlineData(999999.99)]
        public void AddNewEarning_WithValidAmounts_ShouldCreateEarning(decimal amount)
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var result = _sut.AddNewEarning(projectUuid, "Test Earning", amount, DateTime.Now);
            _earningsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(amount, result.Amount);
        }

        [Fact]
        public void AddNewEarning_WithFutureDate_ShouldCreateEarning()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var futureDate = DateTime.Now.AddDays(30);

            // Act
            var result = _sut.AddNewEarning(projectUuid, "Future Earning", 1000m, futureDate);
            _earningsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(futureDate.Date, result.Date.Date);
        }

        [Fact]
        public void AddNewEarning_WithPastDate_ShouldCreateEarning()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var pastDate = DateTime.Now.AddDays(-30);

            // Act
            var result = _sut.AddNewEarning(projectUuid, "Past Earning", 1000m, pastDate);
            _earningsToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pastDate.Date, result.Date.Date);
        }

        [Fact]
        public void AddNewEarning_ShouldSetIsPaidToFalse()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var result = _sut.AddNewEarning(projectUuid, "Test Earning", 1000m, DateTime.Now);
            _earningsToCleanup.Add(result.Uuid);

            // Assert
            Assert.False(result.IsPaid);
        }

        [Fact]
        public void AddNewEarning_ShouldGenerateUniqueUuid()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var earning1 = _sut.AddNewEarning(projectUuid, "Earning 1", 1000m, DateTime.Now);
            var earning2 = _sut.AddNewEarning(projectUuid, "Earning 2", 2000m, DateTime.Now);
            _earningsToCleanup.Add(earning1.Uuid);
            _earningsToCleanup.Add(earning2.Uuid);

            // Assert
            Assert.NotEqual(earning1.Uuid, earning2.Uuid);
            Assert.NotEqual(Guid.Empty, earning1.Uuid);
            Assert.NotEqual(Guid.Empty, earning2.Uuid);
        }

        #endregion

        #region GetByUuid Tests

        [Fact]
        public void GetByUuid_WithExistingEarning_ShouldReturnEarning()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Test Earning", 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Act
            var result = _sut.GetByUuid(earning.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(earning.Uuid, result.Uuid);
            Assert.Equal(earning.Description, result.Description);
            Assert.Equal(earning.Amount, result.Amount);
        }

        [Fact]
        public void GetByUuid_WithEmptyGuid_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _sut.GetByUuid(Guid.Empty));
            Assert.Equal("uuid", exception.ParamName);
            Assert.Contains("UUID cannot be empty", exception.Message);
        }

        [Fact]
        public void GetByUuid_WithNonExistentUuid_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _sut.GetByUuid(nonExistentUuid));
            Assert.Equal("uuid", exception.ParamName);
            Assert.Contains("Earning not found", exception.Message);
        }

        #endregion

        #region Update Tests

        [Fact]
        public void Update_WithValidEarning_ShouldUpdateEarning()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Original Description", 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Act
            earning.Description = "Updated Description";
            earning.Amount = 2000m;
            earning.Date = DateTime.Now.AddDays(5);
            var result = _sut.Update(earning);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(2000m, result.Amount);
        }

        [Fact]
        public void Update_WithNullEarning_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _sut.Update(null));
            Assert.Equal("entity", exception.ParamName);
        }

        [Fact]
        public void Update_WithNonExistentEarning_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentEarning = new Earning(
                Guid.NewGuid(), 
                "Test", 
                1000m, 
                DateTime.Now, 
                false
            );

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _sut.Update(nonExistentEarning));
            Assert.Equal("entity", exception.ParamName);
            Assert.Contains("Earning not found", exception.Message);
        }

        [Fact]
        public void Update_MarkingAsPaid_ShouldUpdateIsPaid()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Test Earning", 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);
            Assert.False(earning.IsPaid);

            // Act
            earning.MarkAsPaid(DateTime.Now);
            var result = _sut.Update(earning);

            // Assert
            Assert.True(result.IsPaid);
        }

        [Fact]
        public void Update_ChangingAmount_ShouldPersistNewAmount()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Test Earning", 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Act
            earning.Amount = 5000m;
            var result = _sut.Update(earning);

            // Assert
            var retrieved = _sut.GetByUuid(earning.Uuid);
            Assert.Equal(5000m, retrieved.Amount);
        }

        [Fact]
        public void Update_ChangingDescription_ShouldPersistNewDescription()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Original", 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Act
            earning.Description = "Completely New Description";
            var result = _sut.Update(earning);

            // Assert
            var retrieved = _sut.GetByUuid(earning.Uuid);
            Assert.Equal("Completely New Description", retrieved.Description);
        }

        [Fact]
        public void Update_ChangingDate_ShouldPersistNewDate()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var originalDate = DateTime.Now.AddDays(-10);
            var earning = _sut.AddNewEarning(projectUuid, "Test", 1000m, originalDate);
            _earningsToCleanup.Add(earning.Uuid);

            // Act
            var newDate = DateTime.Now.AddDays(10);
            earning.Date = newDate;
            var result = _sut.Update(earning);

            // Assert
            var retrieved = _sut.GetByUuid(earning.Uuid);
            Assert.Equal(newDate.Date, retrieved.Date.Date);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void Delete_WithExistingEarning_ShouldDeleteEarning()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Test Earning", 1000m, DateTime.Now);

            // Act
            var result = _sut.Delete(earning);

            // Assert
            Assert.True(result);
            Assert.Throws<ArgumentException>(() => _sut.GetByUuid(earning.Uuid));
        }

        [Fact]
        public void Delete_WithNullEarning_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _sut.Delete(null));
            Assert.Equal("entity", exception.ParamName);
        }

        [Fact]
        public void Delete_WithPaidEarning_ShouldStillDelete()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Test Earning", 1000m, DateTime.Now);
            earning.MarkAsPaid(DateTime.Now);
            _sut.Update(earning);

            // Act
            var result = _sut.Delete(earning);

            // Assert
            Assert.True(result);
            Assert.Throws<ArgumentException>(() => _sut.GetByUuid(earning.Uuid));
        }

        [Fact]
        public void Delete_WithUnpaidEarning_ShouldDelete()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Test Earning", 1000m, DateTime.Now);
            Assert.False(earning.IsPaid);

            // Act
            var result = _sut.Delete(earning);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void EarningWorkflow_CreateUpdateDelete_ShouldWorkCorrectly()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act - Create
            var earning = _sut.AddNewEarning(projectUuid, "Workflow Test", 1000m, DateTime.Now);
            var earningUuid = earning.Uuid;
            Assert.NotNull(earning);

            // Act - Update
            earning.Description = "Updated Workflow";
            earning.Amount = 2000m;
            _sut.Update(earning);
            var updated = _sut.GetByUuid(earningUuid);
            Assert.Equal("Updated Workflow", updated.Description);
            Assert.Equal(2000m, updated.Amount);

            // Act - Delete
            _sut.Delete(earning);

            // Assert
            Assert.Throws<ArgumentException>(() => _sut.GetByUuid(earningUuid));
        }

        [Fact]
        public void CreateMultipleEarnings_ForSameProject_ShouldAllBePersisted()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earnings = new List<Earning>();

            // Act
            for (int i = 1; i <= 5; i++)
            {
                var earning = _sut.AddNewEarning(
                    projectUuid,
                    $"Earning {i}",
                    i * 1000m,
                    DateTime.Now.AddDays(i)
                );
                earnings.Add(earning);
                _earningsToCleanup.Add(earning.Uuid);
            }

            // Assert
            foreach (var earning in earnings)
            {
                var retrieved = _sut.GetByUuid(earning.Uuid);
                Assert.NotNull(retrieved);
                Assert.Equal(earning.Description, retrieved.Description);
            }
        }

        [Fact]
        public void MarkEarningAsPaid_ThenUpdate_ShouldPersist()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Test Earning", 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Act
            var paymentDate = DateTime.Now;
            earning.MarkAsPaid(paymentDate);
            _sut.Update(earning);

            // Assert
            var retrieved = _sut.GetByUuid(earning.Uuid);
            Assert.True(retrieved.IsPaid);
            Assert.Equal(paymentDate.Date, retrieved.Date.Date);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public void MarkAsPaid_ShouldSetIsPaidToTrueAndUpdateDate()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var earning = _sut.AddNewEarning(projectUuid, "Test Earning", 1000m, DateTime.Now.AddDays(-10));
            _earningsToCleanup.Add(earning.Uuid);
            var paymentDate = DateTime.Now;

            // Act
            earning.MarkAsPaid(paymentDate);
            _sut.Update(earning);

            // Assert
            var retrieved = _sut.GetByUuid(earning.Uuid);
            Assert.True(retrieved.IsPaid);
            Assert.Equal(paymentDate.Date, retrieved.Date.Date);
        }

        [Fact]
        public void NewEarning_DefaultState_ShouldBeUnpaid()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var earning = _sut.AddNewEarning(projectUuid, "Test", 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Assert
            Assert.False(earning.IsPaid);
        }

        [Fact]
        public void EarningWithDecimalAmount_ShouldPreservePrecision()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var amount = 1234.56m;

            // Act
            var earning = _sut.AddNewEarning(projectUuid, "Test", amount, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Assert
            var retrieved = _sut.GetByUuid(earning.Uuid);
            Assert.Equal(amount, retrieved.Amount);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void AddNewEarning_WithVeryLongDescription_ShouldCreateEarning()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var longDescription = new string('A', 500);

            // Act
            var earning = _sut.AddNewEarning(projectUuid, longDescription, 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Assert
            Assert.NotNull(earning);
            Assert.Equal(longDescription, earning.Description);
        }

        [Fact]
        public void AddNewEarning_WithSpecialCharactersInDescription_ShouldCreateEarning()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var description = "Konsulentydelse æøå ÆØÅ €$£ 100% #1";

            // Act
            var earning = _sut.AddNewEarning(projectUuid, description, 1000m, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Assert
            var retrieved = _sut.GetByUuid(earning.Uuid);
            Assert.Equal(description, retrieved.Description);
        }

        [Fact]
        public void AddNewEarning_WithMinimumDecimalValue_ShouldCreateEarning()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var amount = 0.01m;

            // Act
            var earning = _sut.AddNewEarning(projectUuid, "Minimum Amount", amount, DateTime.Now);
            _earningsToCleanup.Add(earning.Uuid);

            // Assert
            Assert.Equal(amount, earning.Amount);
        }

        #endregion

        public void Dispose()
        {
            // Cleanup earnings first
            foreach (var earningUuid in _earningsToCleanup)
            {
                try
                {
                    var earning = _earningRepository.GetByUuid(earningUuid);
                    if (earning != null)
                    {
                        _earningRepository.Delete(earningUuid);
                    }
                }
                catch
                {
                    // Ignore if already deleted or other issues
                }
            }

            // Then cleanup projects
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
                    // Ignore if already deleted
                }
            }

            // Finally cleanup clients
            foreach (var clientUuid in _clientsToCleanup)
            {
                try
                {
                    var client = _clientRepository.GetByUuid(clientUuid);
                    if (client != null)
                    {
                        _clientRepository.Delete(clientUuid);
                    }
                }
                catch
                {
                    // Ignore if already deleted
                }
            }
        }
    }
}
