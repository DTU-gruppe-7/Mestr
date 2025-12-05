using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Services.Service;
using Mestr.Services.Interface;
using Mestr.Data.Interface;
using QuestPDF.Infrastructure;

namespace Mestr.Test.Services.Service
{
    /// <summary>
    /// Integration tests for PdfService - Testing core invoice generation functionality
    /// </summary>
    public class PdfServiceTest : IAsyncLifetime
    {
        private readonly IPdfService _sut;
        private readonly ICompanyProfileRepository _companyProfileRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<Earning> _earningRepository;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;
        private readonly List<Guid> _earningsToCleanup;
        private readonly string _testRunId;

        public PdfServiceTest()
        {
            // Configure QuestPDF license for testing
            QuestPDF.Settings.License = LicenseType.Community;
            
            _projectRepository = new ProjectRepository();
            _clientRepository = new ClientRepository();
            _earningRepository = new EarningRepository();
            _companyProfileRepository = new CompanyProfileRepository();
            
            var projectService = new ProjectService(_projectRepository);
            var companyProfileService = new CompanyProfileService(_companyProfileRepository);
            _sut = new PdfService(projectService, companyProfileService);
            
            _projectsToCleanup = new List<Guid>();
            _clientsToCleanup = new List<Guid>();
            _earningsToCleanup = new List<Guid>();
            _testRunId = Guid.NewGuid().ToString().Substring(0, 8);
        }

        public async ValueTask InitializeAsync()
        {
            await EnsureCompanyProfileExistsAsync();
        }

        #region Helper Methods

        private async Task EnsureCompanyProfileExistsAsync()
        {
            try
            {
                var existingProfile = await _companyProfileRepository.GetAsync();
                
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
                    
                    await _companyProfileRepository.SaveAsync(profile);
                }
            }
            catch
            {
                // If profile already exists, continue
            }
        }

        private async Task<Project> CreateTestProjectAsync(bool isBusinessClient = true, bool hasEarnings = true)
        {
            var client = Client.Create(
                Guid.NewGuid(),
                isBusinessClient ? $"PdfTestCompany_{_testRunId}" : $"PdfJohnDoe_{_testRunId}",
                $"PdfPerson_{_testRunId}",
                $"pdftest_{_testRunId}@test.dk",
                "12345678",
                "Testvej 123",
                "1234",
                "Testby",
                isBusinessClient ? "12345678" : null
            );
            await _clientRepository.AddAsync(client);
            _clientsToCleanup.Add(client.Uuid);

            var project = new Project(
                Guid.NewGuid(),
                $"PdfTestProject_{_testRunId}",
                client,
                DateTime.Now.AddDays(-30),
                DateTime.Now.AddDays(-20),
                "Test description",
                ProjectStatus.Aktiv,
                null
            );
            await _projectRepository.AddAsync(project);
            _projectsToCleanup.Add(project.Uuid);

            if (hasEarnings)
            {
                var earning = new Earning(Guid.NewGuid(), $"PdfTestService_{_testRunId}", 1000m, DateTime.Now, false)
                {
                    ProjectUuid = project.Uuid
                };
                await _earningRepository.AddAsync(earning);
                _earningsToCleanup.Add(earning.Uuid);
                
                await Task.Delay(100);
                
                project = await _projectRepository.GetByUuidAsync(project.Uuid);
            }

            return project;
        }

        #endregion

        #region Tests

        [Fact]
        public async Task GeneratePdfInvoice_WithNullProject_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GeneratePdfInvoiceAsync(null));
        }

        [Fact]
        public async Task GeneratePdfInvoice_WithBusinessClient_ShouldGeneratePDF()
        {
            // Arrange
            var project = await CreateTestProjectAsync(isBusinessClient: true);

            // Act
            var result = await _sut.GeneratePdfInvoiceAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GeneratePdfInvoice_WithPrivateClient_ShouldGeneratePDF()
        {
            // Arrange
            var project = await CreateTestProjectAsync(isBusinessClient: false);

            // Act
            var result = await _sut.GeneratePdfInvoiceAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GeneratePdfInvoice_WithNoEarnings_ShouldGeneratePDF()
        {
            // Arrange
            var project = await CreateTestProjectAsync(hasEarnings: false);

            // Act
            var result = await _sut.GeneratePdfInvoiceAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GeneratePdfInvoice_ShouldReturnByteArray()
        {
            // Arrange
            var project = await CreateTestProjectAsync();

            // Act
            var result = await _sut.GeneratePdfInvoiceAsync(project);

            // Assert
            Assert.IsType<byte[]>(result);
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            foreach (var earningUuid in _earningsToCleanup)
            {
                try { await _earningRepository.DeleteAsync(earningUuid); } catch { }
            }

            await Task.Delay(100);

            foreach (var projectUuid in _projectsToCleanup)
            {
                try { await _projectRepository.DeleteAsync(projectUuid); } catch { }
            }

            foreach (var clientUuid in _clientsToCleanup)
            {
                try { await _clientRepository.DeleteAsync(clientUuid); } catch { }
            }

            await Task.Delay(100);
        }
    }
}
