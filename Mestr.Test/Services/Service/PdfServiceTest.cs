using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using QuestPDF.Infrastructure;

namespace Mestr.Test.Services.Service
{
    /// <summary>
    /// Integration tests for PdfService - Testing core invoice generation functionality
    /// </summary>
    public class PdfServiceTest : IDisposable
    {
        private readonly PdfService _sut;
        private readonly CompanyProfileRepository _companyProfileRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly ClientRepository _clientRepository;
        private readonly EarningRepository _earningRepository;
        private readonly List<Guid> _projectsToCleanup;
        private readonly List<Guid> _clientsToCleanup;
        private readonly List<Guid> _earningsToCleanup;
        private readonly string _testRunId;

        public PdfServiceTest()
        {
            // Configure QuestPDF license for testing
            QuestPDF.Settings.License = LicenseType.Community;
            
            _sut = new PdfService();
            _companyProfileRepository = new CompanyProfileRepository();
            _projectRepository = new ProjectRepository();
            _clientRepository = new ClientRepository();
            _earningRepository = new EarningRepository();
            _projectsToCleanup = new List<Guid>();
            _clientsToCleanup = new List<Guid>();
            _earningsToCleanup = new List<Guid>();
            _testRunId = Guid.NewGuid().ToString().Substring(0, 8);

            EnsureCompanyProfileExists();
        }

        #region Helper Methods

        private void EnsureCompanyProfileExists()
        {
            try
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
                }
            }
            catch
            {
                // If profile already exists, continue
            }
        }

        private Project CreateTestProject(bool isBusinessClient = true, bool hasEarnings = true)
        {
            var client = new Client(
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
            _clientRepository.Add(client);
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
            _projectRepository.Add(project);
            _projectsToCleanup.Add(project.Uuid);

            if (hasEarnings)
            {
                var earning = new Earning(Guid.NewGuid(), $"PdfTestService_{_testRunId}", 1000m, DateTime.Now, false)
                {
                    ProjectUuid = project.Uuid
                };
                _earningRepository.Add(earning);
                _earningsToCleanup.Add(earning.Uuid);
                
                System.Threading.Thread.Sleep(100);
                
                project = _projectRepository.GetByUuid(project.Uuid);
            }

            return project;
        }

        #endregion

        #region Tests

        [Fact]
        public void GeneratePdfInvoice_WithNullProject_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _sut.GeneratePdfInvoice(null));
        }

        [Fact]
        public void GeneratePdfInvoice_WithBusinessClient_ShouldGeneratePDF()
        {
            // Arrange
            var project = CreateTestProject(isBusinessClient: true);

            // Act
            var result = _sut.GeneratePdfInvoice(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GeneratePdfInvoice_WithPrivateClient_ShouldGeneratePDF()
        {
            // Arrange
            var project = CreateTestProject(isBusinessClient: false);

            // Act
            var result = _sut.GeneratePdfInvoice(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GeneratePdfInvoice_WithNoEarnings_ShouldGeneratePDF()
        {
            // Arrange
            var project = CreateTestProject(hasEarnings: false);

            // Act
            var result = _sut.GeneratePdfInvoice(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GeneratePdfInvoice_ShouldReturnByteArray()
        {
            // Arrange
            var project = CreateTestProject();

            // Act
            var result = _sut.GeneratePdfInvoice(project);

            // Assert
            Assert.IsType<byte[]>(result);
        }

        #endregion

        public void Dispose()
        {
            foreach (var earningUuid in _earningsToCleanup)
            {
                try { _earningRepository.Delete(earningUuid); } catch { }
            }

            System.Threading.Thread.Sleep(100);

            foreach (var projectUuid in _projectsToCleanup)
            {
                try { _projectRepository.Delete(projectUuid); } catch { }
            }

            foreach (var clientUuid in _clientsToCleanup)
            {
                try { _clientRepository.Delete(clientUuid); } catch { }
            }

            System.Threading.Thread.Sleep(100);
        }
    }
}
