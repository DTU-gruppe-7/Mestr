using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Services.Interface;
using Mestr.Data.Repository;
using Mestr.Data.Interface;

namespace Mestr.Test.Services.Service
{
    /// <summary>
    /// Integration tests for ExpenseService - Testing expense management logic
    /// </summary>
    public class ExpenseServiceTest : IAsyncLifetime
    {
        private readonly IExpenseService _sut;
        private readonly IRepository<Expense> _expenseRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly List<Guid> _expensesToCleanup;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;

        public ExpenseServiceTest()
        {
            _expenseRepository = new ExpenseRepository();
            _projectRepository = new ProjectRepository();
            _clientRepository = new ClientRepository();
            _sut = new Mestr.Services.Service.ExpenseService(_expenseRepository);
            _expensesToCleanup = new List<Guid>();
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
                ProjectStatus.Aktiv,
                null
            );
            await _projectRepository.AddAsync(project);
            _projectsToCleanup.Add(project.Uuid);

            return project.Uuid;
        }

        #endregion

        #region AddNewExpense Tests

        [Fact]
        public async Task AddNewExpense_WithValidData_ShouldCreateExpense()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();
            var description = "Materialer til projekt";
            var amount = 500m;
            var date = DateTime.Now;
            var category = ExpenseCategory.Materialer;

            // Act
            var result = await _sut.AddNewExpenseAsync(projectUuid, description, amount, date, category);
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
        public async Task AddNewExpense_WithEmptyProjectUuid_ShouldThrowArgumentException()
        {
            // Arrange
            var description = "Test Expense";
            var amount = 500m;
            var date = DateTime.Now;
            var category = ExpenseCategory.Materialer;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.AddNewExpenseAsync(Guid.Empty, description, amount, date, category));
            Assert.Equal("projectUuid", exception.ParamName);
            Assert.Contains("Project UUID cannot be empty", exception.Message);
        }

        [Fact]
        public async Task AddNewExpense_WithNullDescription_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.AddNewExpenseAsync(projectUuid, null, 500m, DateTime.Now, ExpenseCategory.Materialer));
            Assert.Equal("description", exception.ParamName);
            Assert.Contains("Description cannot be empty", exception.Message);
        }

        [Fact]
        public async Task AddNewExpense_WithZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.AddNewExpenseAsync(projectUuid, "Description", 0m, DateTime.Now, ExpenseCategory.Materialer));
            Assert.Equal("amount", exception.ParamName);
            Assert.Contains("Amount must be greater than zero", exception.Message);
        }

        [Theory]
        [InlineData(ExpenseCategory.Materialer)]
        [InlineData(ExpenseCategory.Løn)]
        [InlineData(ExpenseCategory.Transport)]
        public async Task AddNewExpense_WithDifferentCategories_ShouldCreateExpense(ExpenseCategory category)
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();

            // Act
            var result = await _sut.AddNewExpenseAsync(projectUuid, "Test Expense", 500m, DateTime.Now, category);
            _expensesToCleanup.Add(result.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category, result.Category);
        }

        #endregion

        #region GetByUuid Tests

        [Fact]
        public async Task GetByUuid_WithExistingExpense_ShouldReturnExpense()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();
            var expense = await _sut.AddNewExpenseAsync(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            var result = await _sut.GetByUuidAsync(expense.Uuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expense.Uuid, result.Uuid);
            Assert.Equal(expense.Description, result.Description);
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
        public async Task Update_WithValidExpense_ShouldUpdateExpense()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();
            var expense = await _sut.AddNewExpenseAsync(projectUuid, "Original Description", 500m, DateTime.Now, ExpenseCategory.Materialer);
            _expensesToCleanup.Add(expense.Uuid);

            // Act
            expense.Description = "Updated Description";
            expense.Amount = 1000m;
            var result = await _sut.UpdateAsync(expense);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(1000m, result.Amount);
        }

        [Fact]
        public async Task Update_WithNullExpense_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAsync(null));
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithExistingExpense_ShouldDeleteExpense()
        {
            // Arrange
            var projectUuid = await CreateTestProjectAsync();
            var expense = await _sut.AddNewExpenseAsync(projectUuid, "Test Expense", 500m, DateTime.Now, ExpenseCategory.Materialer);

            // Act
            var result = await _sut.DeleteAsync(expense);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Delete_WithNullExpense_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteAsync(null));
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            foreach (var expenseUuid in _expensesToCleanup)
            {
                try
                {
                    await _expenseRepository.DeleteAsync(expenseUuid);
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
