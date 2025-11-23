using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Services.Service;
using Mestr.Data.Repository;

namespace Mestr.Test.Services.Service
{
    /// <summary>
    /// Integration tests for ExpenseService - Testing expense management logic
    /// </summary>
    public class ExpenseServiceTest : IDisposable
    {
        private readonly ExpenseService _sut;
        private readonly ExpenseRepository _expenseRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly ClientRepository _clientRepository;
        private readonly List<Guid> _expensesToCleanup;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;

        public ExpenseServiceTest()
        {
            _sut = new ExpenseService();
            _expenseRepository = new ExpenseRepository();
            _projectRepository = new ProjectRepository();
            _clientRepository = new ClientRepository();
            _expensesToCleanup = new List<Guid>();
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
                ProjectStatus.Aktiv,
                null
            );
            _projectRepository.Add(project);
            _projectsToCleanup.Add(project.Uuid);

            return project.Uuid;
        }

        #endregion

        #region AddNewExpense Tests

        [Fact]
        public void AddNewExpense_WithValidData_ShouldCreateExpense()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var description = "Materialer til projekt";
            var amount = 500m;
            var date = DateTime.Now;
            var category = ExpenseCategory.Materialer;

            // Act
            var result = _sut.AddNewExpense(projectUuid, description, amount, date, category);
            _expensesToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(description, result.Description);
            Assert.Equal(amount, result.Amount);
            Assert.Equal(date.Date, result.Date.Date);
            Assert.Equal(category, result.Category);
            Assert.Equal(projectUuid, result.ProjectUuid);
            Assert.False(result.IsAccepted);
        }

        [Fact]
        public void AddNewExpense_WithEmptyProjectUuid_ShouldThrowArgumentException()
        {
            // Arrange
            var description = "Test Expense";
            var amount = 500m;
            var date = DateTime.Now;
            var category = ExpenseCategory.Materialer;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewExpense(Guid.Empty, description, amount, date, category));
            Assert.Equal("projectUuid", exception.ParamName);
            Assert.Contains("Project UUID cannot be empty", exception.Message);
        }

        [Fact]
        public void AddNewExpense_WithNullDescription_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewExpense(projectUuid, null, 500m, DateTime.Now, ExpenseCategory.Materialer));
            Assert.Equal("description", exception.ParamName);
            Assert.Contains("Description cannot be empty", exception.Message);
        }

        [Fact]
        public void AddNewExpense_WithEmptyDescription_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewExpense(projectUuid, "   ", 500m, DateTime.Now, ExpenseCategory.Materialer));
            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public void AddNewExpense_WithWhitespaceDescription_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewExpense(projectUuid, "\t\n", 500m, DateTime.Now, ExpenseCategory.Materialer));
            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public void AddNewExpense_WithZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewExpense(projectUuid, "Description", 0m, DateTime.Now, ExpenseCategory.Materialer));
            Assert.Equal("amount", exception.ParamName);
            Assert.Contains("Amount must be greater than zero", exception.Message);
        }

        [Fact]
        public void AddNewExpense_WithNegativeAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _sut.AddNewExpense(projectUuid, "Description", -100m, DateTime.Now, ExpenseCategory.Materialer));
            Assert.Equal("amount", exception.ParamName);
        }

        [Theory]
        [InlineData(ExpenseCategory.Materialer)]
        [InlineData(ExpenseCategory.Løn)]
        [InlineData(ExpenseCategory.Transport)]
        [InlineData(ExpenseCategory.Værktøj)]
        [InlineData(ExpenseCategory.Underleverandør)]
        [InlineData(ExpenseCategory.Andet)]
        public void AddNewExpense_WithDifferentCategories_ShouldCreateExpense(ExpenseCategory category)
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var result = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, category);
            _expensesToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category, result.Category);
        }

        [Theory]
        [InlineData(0.01)]
        [InlineData(1.00)]
        [InlineData(100.00)]
        [InlineData(500.50)]
        [InlineData(10000.99)]
        [InlineData(999999.99)]
        public void AddNewExpense_WithValidAmounts_ShouldCreateExpense(decimal amount)
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var result = _sut.AddNewExpense(projectUuid, "Test Expense", amount, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(amount, result.Amount);
        }

        [Fact]
        public void AddNewExpense_WithFutureDate_ShouldCreateExpense()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var futureDate = DateTime.Now.AddDays(30);

            // Act
            var result = _sut.AddNewExpense(projectUuid, "Future Expense", 500m, futureDate, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(futureDate.Date, result.Date.Date);
        }

        [Fact]
        public void AddNewExpense_WithPastDate_ShouldCreateExpense()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var pastDate = DateTime.Now.AddDays(-30);

            // Act
            var result = _sut.AddNewExpense(projectUuid, "Past Expense", 500m, pastDate, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pastDate.Date, result.Date.Date);
        }

        [Fact]
        public void AddNewExpense_ShouldSetIsAcceptedToFalse()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var result = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(result.Uuid);

            // Assert
            Assert.False(result.IsAccepted);
        }

        [Fact]
        public void AddNewExpense_ShouldGenerateUniqueUuid()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var expense1 = _sut.AddNewExpense(projectUuid, "Expense 1", 500m, DateTime.Now, ExpenseCategory.Materialer);
            var expense2 = _sut.AddNewExpense(projectUuid, "Expense 2", 1000m, DateTime.Now, ExpenseCategory.Transport);
            _expensesToCleanup.Add(expense1.Uuid);
            _expensesToCleanup.Add(expense2.Uuid);

            // Assert
            Assert.NotEqual(expense1.Uuid, expense2.Uuid);
            Assert.NotEqual(Guid.Empty, expense1.Uuid);
            Assert.NotEqual(Guid.Empty, expense2.Uuid);
        }

        #endregion

        #region GetByUuid Tests

        [Fact]
        public void GetByUuid_WithExistingExpense_ShouldReturnExpense()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            var result = _sut.GetByUuid(expense.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expense.Uuid, result.Uuid);
            Assert.Equal(expense.Description, result.Description);
            Assert.Equal(expense.Amount, result.Amount);
            Assert.Equal(expense.Category, result.Category);
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
            Assert.Contains("Expense not found", exception.Message);
        }

        #endregion

        #region Update Tests

        [Fact]
        public void Update_WithValidExpense_ShouldUpdateExpense()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Original Description", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            expense.Description = "Updated Description";
            expense.Amount = 1000m;
            expense.Category = ExpenseCategory.Transport;
            expense.Date = DateTime.Now.AddDays(5);
            var result = _sut.Update(expense);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(1000m, result.Amount);
            Assert.Equal(ExpenseCategory.Transport, result.Category);
        }

        [Fact]
        public void Update_WithNullExpense_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _sut.Update(null));
            Assert.Equal("entity", exception.ParamName);
        }

        [Fact]
        public void Update_WithNonExistentExpense_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentExpense = new Expense(
                Guid.NewGuid(),
                "Test",
                500m,
                DateTime.Now,
                ExpenseCategory.Materialer,
                false
            );

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _sut.Update(nonExistentExpense));
            Assert.Equal("entity", exception.ParamName);
            Assert.Contains("Expense not found", exception.Message);
        }

        [Fact]
        public void Update_AcceptingExpense_ShouldUpdateIsAccepted()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);
            Assert.False(expense.IsAccepted);

            // Act
            expense.Accept();
            var result = _sut.Update(expense);

            // Assert
            Assert.True(result.IsAccepted);
        }

        [Fact]
        public void Update_ChangingAmount_ShouldPersistNewAmount()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            expense.Amount = 2500m;
            var result = _sut.Update(expense);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.Equal(2500m, retrieved.Amount);
        }

        [Fact]
        public void Update_ChangingDescription_ShouldPersistNewDescription()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Original", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            expense.Description = "Completely New Description";
            var result = _sut.Update(expense);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.Equal("Completely New Description", retrieved.Description);
        }

        [Fact]
        public void Update_ChangingCategory_ShouldPersistNewCategory()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Test", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            expense.Category = ExpenseCategory.Løn;
            var result = _sut.Update(expense);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.Equal(ExpenseCategory.Løn, retrieved.Category);
        }

        [Fact]
        public void Update_ChangingDate_ShouldPersistNewDate()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var originalDate = DateTime.Now.AddDays(-10);
            var expense = _sut.AddNewExpense(projectUuid, "Test", 500m, originalDate, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            var newDate = DateTime.Now.AddDays(10);
            expense.Date = newDate;
            var result = _sut.Update(expense);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.Equal(newDate.Date, retrieved.Date.Date);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void Delete_WithExistingExpense_ShouldDeleteExpense()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);

            // Act
            var result = _sut.Delete(expense);

            // Assert
            Assert.True(result);
            Assert.Throws<ArgumentException>(() => _sut.GetByUuid(expense.Uuid));
        }

        [Fact]
        public void Delete_WithNullExpense_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _sut.Delete(null));
            Assert.Equal("entity", exception.ParamName);
        }

        [Fact]
        public void Delete_WithAcceptedExpense_ShouldStillDelete()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);
            expense.Accept();
            _sut.Update(expense);

            // Act
            var result = _sut.Delete(expense);

            // Assert
            Assert.True(result);
            Assert.Throws<ArgumentException>(() => _sut.GetByUuid(expense.Uuid));
        }

        [Fact]
        public void Delete_WithUnacceptedExpense_ShouldDelete()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);
            Assert.False(expense.IsAccepted);

            // Act
            var result = _sut.Delete(expense);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ExpenseWorkflow_CreateUpdateDelete_ShouldWorkCorrectly()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act - Create
            var expense = _sut.AddNewExpense(projectUuid, "Workflow Test", 500m, DateTime.Now, ExpenseCategory.Materialer);
            var expenseUuid = expense.Uuid;
            Assert.NotNull(expense);

            // Act - Update
            expense.Description = "Updated Workflow";
            expense.Amount = 1000m;
            expense.Category = ExpenseCategory.Transport;
            _sut.Update(expense);
            var updated = _sut.GetByUuid(expenseUuid);
            Assert.Equal("Updated Workflow", updated.Description);
            Assert.Equal(1000m, updated.Amount);
            Assert.Equal(ExpenseCategory.Transport, updated.Category);

            // Act - Delete
            _sut.Delete(expense);

            // Assert
            Assert.Throws<ArgumentException>(() => _sut.GetByUuid(expenseUuid));
        }

        [Fact]
        public void CreateMultipleExpenses_ForSameProject_ShouldAllBePersisted()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expenses = new List<Expense>();
            var categories = new[] 
            { 
                ExpenseCategory.Materialer, 
                ExpenseCategory.Transport, 
                ExpenseCategory.Løn, 
                ExpenseCategory.Værktøj,
                ExpenseCategory.Underleverandør,
                ExpenseCategory.Andet 
            };

            // Act
            for (int i = 0; i < categories.Length; i++)
            {
                var expense = _sut.AddNewExpense(
                    projectUuid,
                    $"Expense {i + 1}",
                    (i + 1) * 100m,
                    DateTime.Now.AddDays(i),
                    categories[i]
                );
                expenses.Add(expense);
                _expensesToCleanup.Add(expense.Uuid);
            }

            // Assert
            foreach (var expense in expenses)
            {
                var retrieved = _sut.GetByUuid(expense.Uuid);
                Assert.NotNull(retrieved);
                Assert.Equal(expense.Description, retrieved.Description);
                Assert.Equal(expense.Category, retrieved.Category);
            }
        }

        [Fact]
        public void AcceptExpense_ThenUpdate_ShouldPersist()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            expense.Accept();
            _sut.Update(expense);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.True(retrieved.IsAccepted);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public void Accept_ShouldSetIsAcceptedToTrue()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            expense.Accept();
            _sut.Update(expense);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.True(retrieved.IsAccepted);
        }

        [Fact]
        public void NewExpense_DefaultState_ShouldBeUnaccepted()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var expense = _sut.AddNewExpense(projectUuid, "Test", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Assert
            Assert.False(expense.IsAccepted);
        }

        [Fact]
        public void ExpenseWithDecimalAmount_ShouldPreservePrecision()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var amount = 1234.56m;

            // Act
            var expense = _sut.AddNewExpense(projectUuid, "Test", amount, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.Equal(amount, retrieved.Amount);
        }

        [Fact]
        public void ExpenseCategory_ShouldBePersisted()
        {
            // Arrange
            var projectUuid = CreateTestProject();

            // Act
            var expense = _sut.AddNewExpense(projectUuid, "Test", 500m, DateTime.Now, ExpenseCategory.Værktøj);
            _expensesToCleanup.Add(expense.Uuid);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.Equal(ExpenseCategory.Værktøj, retrieved.Category);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void AddNewExpense_WithVeryLongDescription_ShouldCreateExpense()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var longDescription = new string('A', 500);

            // Act
            var expense = _sut.AddNewExpense(projectUuid, longDescription, 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Assert
            Assert.NotNull(expense);
            Assert.Equal(longDescription, expense.Description);
        }

        [Fact]
        public void AddNewExpense_WithSpecialCharactersInDescription_ShouldCreateExpense()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var description = "Materialer æøå ÆØÅ €$£ 100% #1";

            // Act
            var expense = _sut.AddNewExpense(projectUuid, description, 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.Equal(description, retrieved.Description);
        }

        [Fact]
        public void AddNewExpense_WithMinimumDecimalValue_ShouldCreateExpense()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var amount = 0.01m;

            // Act
            var expense = _sut.AddNewExpense(projectUuid, "Minimum Amount", amount, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Assert
            Assert.Equal(amount, expense.Amount);
        }

        [Fact]
        public void UpdateExpense_MultipleTimesInSuccession_ShouldPersistLatestChanges()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var expense = _sut.AddNewExpense(projectUuid, "Original", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            expense.Description = "First Update";
            _sut.Update(expense);

            expense.Description = "Second Update";
            _sut.Update(expense);

            expense.Description = "Final Update";
            _sut.Update(expense);

            // Assert
            var retrieved = _sut.GetByUuid(expense.Uuid);
            Assert.Equal("Final Update", retrieved.Description);
        }

        #endregion

        #region Category Tests

        [Fact]
        public void ExpenseCategories_AllValues_ShouldBeSupported()
        {
            // Arrange
            var projectUuid = CreateTestProject();
            var allCategories = Enum.GetValues<ExpenseCategory>();

            // Act & Assert
            foreach (var category in allCategories)
            {
                var expense = _sut.AddNewExpense(
                    projectUuid,
                    $"Test for {category}",
                    500m,
                    DateTime.Now,
                    category
                );
                _expensesToCleanup.Add(expense.Uuid);

                Assert.Equal(category, expense.Category);
            }
        }

        #endregion

        public void Dispose()
        {
            // Cleanup expenses first
            foreach (var expenseUuid in _expensesToCleanup)
            {
                try
                {
                    var expense = _expenseRepository.GetByUuid(expenseUuid);
                    if (expense != null)
                    {
                        _expenseRepository.Delete(expenseUuid);
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
