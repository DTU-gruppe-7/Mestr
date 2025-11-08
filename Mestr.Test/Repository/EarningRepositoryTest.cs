using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Data.Interface;


namespace Mestr.Test.Repository
{
    public class EarningRepositoryTest //tester, at man kan tilføje Earning til et repository og derefter hente den igen (GetByUuid)
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Earning> _earningRepository;
        private readonly Project testProject;

        public EarningRepositoryTest()
        {
            _projectRepository = new ProjectRepository();
            _earningRepository = new EarningRepository();

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

            var testEarning = new Earning(
                Guid.NewGuid(),
                testProject.Uuid,
                "Test Earning Description",
                1500.00m,
                DateTime.Now,
                false
            );

            // Act
            _earningRepository.Add(testEarning);

            // Assert
            Earning retrievedEarning = _earningRepository.GetByUuid(testEarning.Uuid);
            Assert.NotNull(retrievedEarning);
            Assert.Equal(testEarning.Uuid, retrievedEarning.Uuid);
            Assert.Equal(testEarning.ProjectUuid, retrievedEarning.ProjectUuid);
            Assert.Equal(testEarning.Description, retrievedEarning.Description);
            Assert.Equal(testEarning.Amount, retrievedEarning.Amount);
            Assert.Equal(testEarning.Date.ToString("yyyy-MM-dd HH:mm:ss"), retrievedEarning.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.False(retrievedEarning.IsPaid);

            // Cleanup
            _earningRepository.Delete(testEarning.Uuid);
        }
        [Fact]
        public void GetByUuid_NonExistentEarning_ReturnsNull()
        {
            // Arrange
            var nonExistentUuid = Guid.NewGuid();
            // Act
            Earning retrievedEarning = _earningRepository.GetByUuid(nonExistentUuid);
            // Assert
            Assert.Null(retrievedEarning);
        }

        [Fact]
        public void Add_NullEarning_ThrowsArgumentNullException()
        {
            // Arrange
            Earning nullEarning = null;
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _earningRepository.Add(nullEarning));
        }

        [Fact]
        public void GetByUuid_EmptyGuid_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _earningRepository.GetByUuid(emptyGuid));
        }

        [Fact]
        public void GetAll_EarningsExist_ReturnsAllEarnings()
        {
            // Arrange
            var testEarning1 = new Earning(
                Guid.NewGuid(),
                testProject.Uuid,
                "Test Earning 1",
                1000.00m,
                DateTime.Now,
                false
            );
            var testEarning2 = new Earning(
                Guid.NewGuid(),
                testProject.Uuid,
                "Test Earning 2",
                2000.00m,
                DateTime.Now,
                true
            );
            _earningRepository.Add(testEarning1);
            _earningRepository.Add(testEarning2);
            // Act
            var allEarnings = _earningRepository.GetAll().ToList();
            // Assert
            Assert.Contains(allEarnings, e => e.Uuid == testEarning1.Uuid);
            Assert.Contains(allEarnings, e => e.Uuid == testEarning2.Uuid);
            // Cleanup
            _earningRepository.Delete(testEarning1.Uuid);
            _earningRepository.Delete(testEarning2.Uuid);
        }

        [Fact]
        public void Delete_ExistentEarning_RemovesEarning()
        {
            // Arrange
            var testEarning = new Earning(
                Guid.NewGuid(),
                testProject.Uuid,
                "Earning to be deleted",
                500.00m,
                DateTime.Now,
                false
            );
            _earningRepository.Add(testEarning);
            // Act
            _earningRepository.Delete(testEarning.Uuid);
            // Assert
            Earning retrievedEarning = _earningRepository.GetByUuid(testEarning.Uuid);
            Assert.Null(retrievedEarning);
        }

        [Fact]
        public void Update_ExistentEarning_UpdatesEarning()
        {
            // Arrange
            var testEarning = new Earning(
                Guid.NewGuid(),
                testProject.Uuid,
                "Original Description",
                750.00m,
                DateTime.Now,
                false
            );
            _earningRepository.Add(testEarning);
            // Act
            testEarning.Description = "Updated Description";
            testEarning.Amount = 1250.00m;
            testEarning.IsPaid = true;
            _earningRepository.Update(testEarning);
            // Assert
            Earning retrievedEarning = _earningRepository.GetByUuid(testEarning.Uuid);
            Assert.Equal("Updated Description", retrievedEarning.Description);
            Assert.Equal(1250.00m, retrievedEarning.Amount);
            Assert.True(retrievedEarning.IsPaid);
            // Cleanup
            _earningRepository.Delete(testEarning.Uuid);
        }

        [Fact]
        public void Cleanup_TestProject()
        {
            // Cleanup
            _projectRepository.Delete(testProject.Uuid);
        }
    }
}
