using System;
using Xunit;
using Mestr.Core.Model;
using Mestr.Services.Service;
using Mestr.Data.Repository;

namespace Mestr.Test.Services.Service
{
    /// <summary>
    /// Integration tests for CompanyProfileService - Testing company profile management logic
    /// </summary>
    public class CompanyProfileServiceTest : IDisposable
    {
        private readonly CompanyProfileService _sut;
        private readonly CompanyProfileRepository _companyProfileRepository;
        private Guid? _profileToCleanup;

        public CompanyProfileServiceTest()
        {
            _sut = new CompanyProfileService();
            _companyProfileRepository = new CompanyProfileRepository();
            
            // Ensure a profile exists for testing
            EnsureProfileExists();
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
                profile.Address,
                profile.ZipCode,
                profile.City,
                profile.Cvr,
                profile.PhoneNumber,
                profile.BankRegNumber,
                profile.BankAccountNumber
            };

            // Act
            profile.CompanyName = $"Test Firma {timestamp}";
            profile.Email = $"test{timestamp}@firma.dk";
            profile.Address = $"Testvej {timestamp}";
            profile.ZipCode = "2000";
            profile.City = "Frederiksberg";
            profile.Cvr = "87654321";
            profile.PhoneNumber = "87654321";
            profile.BankRegNumber = "4321";
            profile.BankAccountNumber = "87654321";
            
            _sut.UpdateProfile(profile);

            // Assert
            var saved = _sut.GetProfile();
            Assert.Equal($"Test Firma {timestamp}", saved.CompanyName);
            Assert.Equal($"test{timestamp}@firma.dk", saved.Email);
            Assert.Equal($"Testvej {timestamp}", saved.Address);
            Assert.Equal("2000", saved.ZipCode);
            Assert.Equal("Frederiksberg", saved.City);
            Assert.Equal("87654321", saved.Cvr);
            
            // Restore original values
            saved.CompanyName = originalValues.CompanyName;
            saved.Email = originalValues.Email;
            saved.Address = originalValues.Address;
            saved.ZipCode = originalValues.ZipCode;
            saved.City = originalValues.City;
            saved.Cvr = originalValues.Cvr;
            saved.PhoneNumber = originalValues.PhoneNumber;
            saved.BankRegNumber = originalValues.BankRegNumber;
            saved.BankAccountNumber = originalValues.BankAccountNumber;
            _sut.UpdateProfile(saved);
        }

        [Fact]
        public void UpdateProfile_MultipleTimesInSuccession_ShouldPersistLatestChanges()
        {
            // Arrange
            var profile = _sut.GetProfile();
            var originalName = profile.CompanyName;

            // Act
            profile.CompanyName = "First Update";
            _sut.UpdateProfile(profile);

            profile.CompanyName = "Second Update";
            _sut.UpdateProfile(profile);

            profile.CompanyName = "Final Update";
            _sut.UpdateProfile(profile);

            // Assert
            var retrieved = _sut.GetProfile();
            Assert.Equal("Final Update", retrieved.CompanyName);
            
            // Restore
            retrieved.CompanyName = originalName;
            _sut.UpdateProfile(retrieved);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void CompanyProfileWorkflow_GetUpdateGet_ShouldWorkCorrectly()
        {
            // Arrange - Get
            var profile = _sut.GetProfile();
            var originalEmail = profile.Email;
            Assert.NotNull(profile);

            // Act - Update
            var newEmail = $"updated{DateTime.Now.Ticks}@test.dk";
            profile.Email = newEmail;
            _sut.UpdateProfile(profile);
            
            var updated = _sut.GetProfile();

            // Assert
            Assert.Equal(newEmail, updated.Email);
            
            // Restore original
            updated.Email = originalEmail;
            _sut.UpdateProfile(updated);
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

        [Fact]
        public void CompanyProfile_OptionalFields_CanBeNullOrEmpty()
        {
            // Arrange
            var profile = _sut.GetProfile();
            var originalAddress = profile.Address;
            var originalZipCode = profile.ZipCode;

            // Act - Set optional fields to empty
            profile.Address = string.Empty;
            profile.ZipCode = string.Empty;
            _sut.UpdateProfile(profile);

            // Assert
            var updated = _sut.GetProfile();
            Assert.NotNull(updated);
            
            // Restore
            updated.Address = originalAddress;
            updated.ZipCode = originalZipCode;
            _sut.UpdateProfile(updated);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void UpdateProfile_WithSpecialCharacters_ShouldPersist()
        {
            // Arrange
            var profile = _sut.GetProfile();
            var originalName = profile.CompanyName;
            var specialName = "Firma æøå ÆØÅ & Co. ApS";

            // Act
            profile.CompanyName = specialName;
            _sut.UpdateProfile(profile);

            // Assert
            var retrieved = _sut.GetProfile();
            Assert.Equal(specialName, retrieved.CompanyName);
            
            // Restore
            retrieved.CompanyName = originalName;
            _sut.UpdateProfile(retrieved);
        }

        #endregion

        public void Dispose()
        {
            // Note: We generally don't delete the company profile as it's a singleton
            // and other tests or the application might depend on it.
            // If a profile was created specifically for testing, we could clean it up here,
            // but since we're using the existing profile, we just ensure it's in a good state.
            
            // Optionally reset to a known good state
            try
            {
                var profile = _sut.GetProfile();
                if (profile != null && _profileToCleanup.HasValue && profile.Uuid == _profileToCleanup.Value)
                {
                    // Only cleanup if we created a new profile during test setup
                    // For now, we leave the profile as other tests might need it
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
