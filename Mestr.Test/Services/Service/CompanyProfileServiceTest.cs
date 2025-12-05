using System;
using System.Threading.Tasks;
using Xunit;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Data.Interface;
using Mestr.Data.Repository;

namespace Mestr.Test.Services.Service
{
    /// <summary>
    /// Integration tests for CompanyProfileService - Testing company profile management logic
    /// </summary>
    public class CompanyProfileServiceTest : IAsyncLifetime
    {
        private readonly ICompanyProfileService _sut;
        private readonly ICompanyProfileRepository _companyProfileRepository;
        private Guid? _profileToCleanup;

        public CompanyProfileServiceTest()
        {
            _companyProfileRepository = new CompanyProfileRepository();
            _sut = new Mestr.Services.Service.CompanyProfileService(_companyProfileRepository);
        }

        public ValueTask InitializeAsync()
        {
            EnsureProfileExists();
            return ValueTask.CompletedTask;
        }

        #region Helper Methods

        private CompanyProfile EnsureProfileExists()
        {
            var existingProfile = _companyProfileRepository.Get();
            
            if (existingProfile == null)
            {
                var profile = new CompanyProfile("Test Firma ApS", "test@firma.dk")
                {
                    Address = "Testvej 100",
                    ZipCode = "1000",
                    City = "København",
                    Cvr = "12345678",
                    PhoneNumber = "12345678",
                    BankRegNumber = "1234",
                    BankAccountNumber = "12345678"
                };
                
                _companyProfileRepository.Save(profile);
                _profileToCleanup = profile.Uuid;
                return profile;
            }
            
            return existingProfile;
        }

        #endregion

        #region GetProfile Tests

        [Fact]
        public void GetProfile_ShouldReturnProfile()
        {
            // Act
            var result = _sut.GetProfile();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.CompanyName);
            Assert.NotNull(result.Email);
        }

        [Fact]
        public void GetProfile_ShouldReturnProfileWithUuid()
        {
            // Act
            var result = _sut.GetProfile();

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Uuid);
        }

        #endregion

        #region UpdateProfile Tests

        [Fact]
        public void UpdateProfile_WithValidProfile_ShouldUpdateProfile()
        {
            // Arrange
            var profile = _sut.GetProfile();
            var originalName = profile.CompanyName;
            var updatedName = $"Updated Company {DateTime.Now.Ticks}";
            
            profile.CompanyName = updatedName;

            // Act
            _sut.UpdateProfile(profile);

            // Assert
            var updated = _sut.GetProfile();
            Assert.Equal(updatedName, updated.CompanyName);
            
            // Restore original name
            updated.CompanyName = originalName;
            _sut.UpdateProfile(updated);
        }

        [Fact]
        public void UpdateProfile_WithCompleteData_ShouldSaveAllFields()
        {
            // Arrange
            var profile = _sut.GetProfile();
            var timestamp = DateTime.Now.Ticks.ToString();
            
            var originalValues = new
            {
                profile.CompanyName,
                profile.Email,
                profile.Address
            };

            // Act
            profile.CompanyName = $"Test Firma {timestamp}";
            profile.Email = $"test{timestamp}@firma.dk";
            profile.Address = $"Testvej {timestamp}";
            
            _sut.UpdateProfile(profile);

            // Assert
            var saved = _sut.GetProfile();
            Assert.Equal($"Test Firma {timestamp}", saved.CompanyName);
            Assert.Equal($"test{timestamp}@firma.dk", saved.Email);
            Assert.Equal($"Testvej {timestamp}", saved.Address);
            
            // Restore original values
            saved.CompanyName = originalValues.CompanyName;
            saved.Email = originalValues.Email;
            saved.Address = originalValues.Address;
            _sut.UpdateProfile(saved);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public void CompanyProfile_RequiredFields_ShouldBePersisted()
        {
            // Arrange
            var profile = _sut.GetProfile();

            // Assert - Required fields should not be null
            Assert.NotNull(profile.CompanyName);
            Assert.NotEmpty(profile.CompanyName);
            Assert.NotNull(profile.Email);
            Assert.NotEmpty(profile.Email);
        }

        #endregion

        public ValueTask DisposeAsync()
        {
            // Note: We generally don't delete the company profile as it's a singleton
            return ValueTask.CompletedTask;
        }
    }
}
