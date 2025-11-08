using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Data.Interface;

namespace Mestr.Test.Repository
{
    public class ExpenseRepositoryTest
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Expense> _expenseRepository;
        private readonly Project testProject;

        public ExpenseRepositoryTest()
        {
            _projectRepository = new ProjectRepository();
            _expenseRepository = new ExpenseRepository();

            testProject = new Project(
                Guid.NewGuid(),
                "Test Project",
                DateTime.Now,
                DateTime.Now.AddMonths(1),
                "This is a testproject",
                ProjectStatus.Ongoing,
                DateTime.Now.AddMonths(10)
            );
            _projectRepository.Add(testProject);
        }

        [Fact]

        public void AddToRepositoryTest()
        {
            // Arrange
            var testExpense = new Expense(
                Guid.NewGuid(),
                testProject.Uuid,
                "Test Expense Description",
                750.00m,
                DateTime.Now,
                ExpenseCategory.Materials,
                false
            );
            // Act
            _expenseRepository.Add(testExpense);
            // Assert
            Expense retrievedExpense = _expenseRepository.GetByUuid(testExpense.Uuid);
            Assert.NotNull(retrievedExpense);
            Assert.Equal(testExpense.Uuid, retrievedExpense.Uuid);
            Assert.Equal(testExpense.ProjectUuid, retrievedExpense.ProjectUuid);
            Assert.Equal(testExpense.Description, retrievedExpense.Description);
            Assert.Equal(testExpense.Amount, retrievedExpense.Amount);
            Assert.Equal(testExpense.Date.ToString("yyyy-MM-dd HH:mm:ss"), retrievedExpense.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.False(retrievedExpense.IsAccepted);
            // Cleanup
            _expenseRepository.Delete(testExpense.Uuid);
        }
        [Fact]

        public void GetByUuid_NonExistentUuid_ThrowsException()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _expenseRepository.GetByUuid(nonExistentUuid));
        }

        [Fact]
        public void Add_NullExpense_ThrowsArgumentNullException()
        {
            // Arrange
            Expense nullExpense = null;
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _expenseRepository.Add(nullExpense));
        }

        [Fact]
        public void GetByUuid_EmptyGuid_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _expenseRepository.GetByUuid(emptyGuid));
        }

        [Fact]
        public void GetAll_ExpensesExist_ReturnsAllExpenses()
        {
            // Arrange
            var testExpense1 = new Expense(
                Guid.NewGuid(),
                testProject.Uuid,
                "Test Expense 1",
                500.00m,
                DateTime.Now,
                ExpenseCategory.Materials,
                false
            );
            var testExpense2 = new Expense(
                Guid.NewGuid(),
                testProject.Uuid,
                "Test Expense 2",
                300.00m,
                DateTime.Now,
                ExpenseCategory.Materials,
                true
            );
            _expenseRepository.Add(testExpense1);
            _expenseRepository.Add(testExpense2);
            // Act
            var allExpenses = _expenseRepository.GetAll().ToList();
            // Assert
            Assert.Contains(allExpenses, e => e.Uuid == testExpense1.Uuid);
            Assert.Contains(allExpenses, e => e.Uuid == testExpense2.Uuid);
            // Cleanup
            _expenseRepository.Delete(testExpense1.Uuid);
            _expenseRepository.Delete(testExpense2.Uuid);
        }

        [Fact]
        public void Delete_ExistingExpense_RemovesExpense()
        {
            // Arrange
            var testExpense = new Expense(
                Guid.NewGuid(),
                testProject.Uuid,
                "Test Expense to Delete",
                400.00m,
                DateTime.Now,
                ExpenseCategory.Materials,
                false
            );
            _expenseRepository.Add(testExpense);
            // Act
            _expenseRepository.Delete(testExpense.Uuid);
            // Assert
            var retrievedExpense = _expenseRepository.GetByUuid(testExpense.Uuid);
            Assert.Null(retrievedExpense);
        }

        [Fact]
        public void Update_ExistingExpense_UpdatesExpense()
        {
            // Arrange
            var testExpense = new Expense(
                Guid.NewGuid(),
                testProject.Uuid,
                "Test Expense to Update",
                600.00m,
                DateTime.Now,
                ExpenseCategory.Materials,
                false
            );
            _expenseRepository.Add(testExpense);
            // Act
            testExpense.Description = "Updated Expense Description";
            testExpense.Amount = 800.00m;
            testExpense.IsAccepted = true;
            _expenseRepository.Update(testExpense);
            // Assert
            var updatedExpense = _expenseRepository.GetByUuid(testExpense.Uuid);
            Assert.Equal("Updated Expense Description", updatedExpense.Description);
            Assert.Equal(800.00m, updatedExpense.Amount);
            Assert.True(updatedExpense.IsAccepted);
            // Cleanup
            _expenseRepository.Delete(testExpense.Uuid);
        }

        [Fact]
        public void Cleanup()
        {
            _projectRepository.Delete(testProject.Uuid);
        }
    }
}
