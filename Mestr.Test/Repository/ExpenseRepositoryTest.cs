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

        private Project CreateTestProject()
        {
            var client = new Client(Guid.NewGuid(), "Test Client", "John Doe", "test@something.com", "12345678", "123 Test St", "12345", "Test City", "88888888");
            _clientRepository.AddAsync(client).Wait();
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
            _projectRepository.AddAsync(project).Wait();
            _projectsToCleanup.Add(project.Uuid);
            return project;
        }

        [Fact]
        public void AddToRepositoryTest()
        {
            // Arrange
            var testProject = CreateTestProject();
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
            _expenseRepository.AddAsync(testExpense).Wait();

            // Assert
            Expense? retrievedExpense = _expenseRepository.GetByUuidAsync(testExpense.Uuid).Result;
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
        public void GetByUuid_NonExistentUuid_ReturnNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();

            // Act
            Expense? retrievedExpense = _expenseRepository.GetByUuidAsync(nonExistentUuid).Result;

            // Assert
            Assert.Null(retrievedExpense);
        }

        [Fact]
        public void Add_NullExpense_ThrowsArgumentNullException()
        {
            // Arrange
            Expense? nullExpense = null;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _expenseRepository.AddAsync(nullExpense)).Wait();
        }

        [Fact]
        public void GetByUuid_EmptyGuid_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _expenseRepository.GetByUuidAsync(emptyGuid)).Wait();
        }

        [Fact]
        public void GetAll_ExpensesExist_ReturnsAllExpenses()
        {
            // Arrange
            var testProject = CreateTestProject();
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

            _expenseRepository.AddAsync(testExpense1).Wait();
            _expenseRepository.AddAsync(testExpense2).Wait();

            // Act
            var allExpenses = _expenseRepository.GetAllAsync().Result.ToList();

            // Assert
            Assert.Contains(allExpenses, e => e.Uuid == testExpense1.Uuid);
            Assert.Contains(allExpenses, e => e.Uuid == testExpense2.Uuid);
        }

        [Fact]
        public void Delete_ExistingExpense_RemovesExpense()
        {
            // Arrange
            var testProject = CreateTestProject();
            var testExpense = new Expense(
                Guid.NewGuid(),
                "Test Expense to Delete",
                400.00m,
                DateTime.Now,
                ExpenseCategory.Materialer,
                false
            );
            testExpense.ProjectUuid = testProject.Uuid;
            _expenseRepository.AddAsync(testExpense).Wait();

            // Act
            _expenseRepository.DeleteAsync(testExpense.Uuid).Wait();

            // Assert
            var retrievedExpense = _expenseRepository.GetByUuidAsync(testExpense.Uuid).Result;
            Assert.Null(retrievedExpense);
        }

        [Fact]
        public void Update_ExistingExpense_UpdatesExpense()
        {
            // Arrange
            var testProject = CreateTestProject();
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
            _expenseRepository.AddAsync(testExpense).Wait();

            // Act
            testExpense.Description = "Updated Expense Description";
            testExpense.Amount = 800.00m;
            testExpense.IsAccepted = true;
            _expenseRepository.UpdateAsync(testExpense).Wait();

            // Assert
            var updatedExpense = _expenseRepository.GetByUuidAsync(testExpense.Uuid).Result;
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
