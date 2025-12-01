using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Data.Interface;
using Mestr.Data.DbContext;

namespace Mestr.Test.Repository
{
    public class ExpenseRepositoryTest : IAsyncLifetime
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Expense> _expenseRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly List<Guid> _expensesToCleanup;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;

        public ExpenseRepositoryTest()
        {
            _projectRepository = new ProjectRepository();
            _expenseRepository = new ExpenseRepository();
            _clientRepository = new ClientRepository();
            _expensesToCleanup = new List<Guid>();
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
            var testExpense = new Expense(
                Guid.NewGuid(),
                "Test Expense Description",
                750.00m,
                DateTime.Now,
                ExpenseCategory.Materialer,
                false
            );
            testExpense.ProjectUuid = testProject.Uuid;
            _expensesToCleanup.Add(testExpense.Uuid);

            // Act
            await _expenseRepository.AddAsync(testExpense);

            // Assert
            Expense? retrievedExpense = await _expenseRepository.GetByUuidAsync(testExpense.Uuid);
            Assert.NotNull(retrievedExpense);
            Assert.Equal(testExpense.Uuid, retrievedExpense.Uuid);
            Assert.Equal(testExpense.ProjectUuid, retrievedExpense.ProjectUuid);
            Assert.Equal(testExpense.Description, retrievedExpense.Description);
            Assert.Equal(testExpense.Amount, retrievedExpense.Amount);
            Assert.Equal(testExpense.Date.ToString("yyyy-MM-dd HH:mm:ss"), retrievedExpense.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.False(retrievedExpense.IsAccepted);
            Assert.NotNull(retrievedExpense.Project);
            Assert.Equal(testProject.Uuid, retrievedExpense.Project.Uuid);
        }

        [Fact]
        public async Task GetByUuid_NonExistentUuid_ReturnNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act
            Expense? retrievedExpense = await _expenseRepository.GetByUuidAsync(nonExistentUuid);

            // Assert
            Assert.Null(retrievedExpense);
        }

        [Fact]
        public async Task Add_NullExpense_ThrowsArgumentNullException()
        {
            // Arrange
            Expense? nullExpense = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _expenseRepository.AddAsync(nullExpense));
        }

        [Fact]
        public async Task GetByUuid_EmptyGuid_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _expenseRepository.GetByUuidAsync(emptyGuid));
        }

        [Fact]
        public async Task GetAll_ExpensesExist_ReturnsAllExpenses()
        {
            // Arrange
            var testProject = await CreateTestProjectAsync();
            var testExpense1 = new Expense(
                Guid.NewGuid(),
                "Test Expense 1",
                500.00m,
                DateTime.Now,
                ExpenseCategory.Materialer,
                false
            );
            testExpense1.ProjectUuid = testProject.Uuid;
            
            var testExpense2 = new Expense(
                Guid.NewGuid(),
                "Test Expense 2",
                300.00m,
                DateTime.Now,
                ExpenseCategory.Materialer,
                true
            );
            testExpense2.ProjectUuid = testProject.Uuid;
            
            _expensesToCleanup.Add(testExpense1.Uuid);
            _expensesToCleanup.Add(testExpense2.Uuid);

            await _expenseRepository.AddAsync(testExpense1);
            await _expenseRepository.AddAsync(testExpense2);

            // Act
            var allExpenses = (await _expenseRepository.GetAllAsync()).ToList();

            // Assert
            Assert.Contains(allExpenses, e => e.Uuid == testExpense1.Uuid);
            Assert.Contains(allExpenses, e => e.Uuid == testExpense2.Uuid);
        }

        [Fact]
        public async Task Delete_ExistingExpense_RemovesExpense()
        {
            // Arrange
            var testProject = await CreateTestProjectAsync();
            var testExpense = new Expense(
                Guid.NewGuid(),
                "Test Expense to Delete",
                400.00m,
                DateTime.Now,
                ExpenseCategory.Materialer,
                false
            );
            testExpense.ProjectUuid = testProject.Uuid;
            await _expenseRepository.AddAsync(testExpense);

            // Act
            await _expenseRepository.DeleteAsync(testExpense.Uuid);

            // Assert
            var retrievedExpense = await _expenseRepository.GetByUuidAsync(testExpense.Uuid);
            Assert.Null(retrievedExpense);
        }

        [Fact]
        public async Task Update_ExistingExpense_UpdatesExpense()
        {
            // Arrange
            var testProject = await CreateTestProjectAsync();
            var testExpense = new Expense(
                Guid.NewGuid(),
                "Test Expense to Update",
                600.00m,
                DateTime.Now,
                ExpenseCategory.Materialer,
                false
            );
            testExpense.ProjectUuid = testProject.Uuid;
            _expensesToCleanup.Add(testExpense.Uuid);
            await _expenseRepository.AddAsync(testExpense);

            // Act
            testExpense.Description = "Updated Expense Description";
            testExpense.Amount = 800.00m;
            testExpense.IsAccepted = true;
            await _expenseRepository.UpdateAsync(testExpense);

            // Assert
            var updatedExpense = await _expenseRepository.GetByUuidAsync(testExpense.Uuid);
            Assert.Equal("Updated Expense Description", updatedExpense.Description);
            Assert.Equal(800.00m, updatedExpense.Amount);
            Assert.True(updatedExpense.IsAccepted);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var expenseUuid in _expensesToCleanup)
            {
                try
                {
                    await _expenseRepository.DeleteAsync(expenseUuid);
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
