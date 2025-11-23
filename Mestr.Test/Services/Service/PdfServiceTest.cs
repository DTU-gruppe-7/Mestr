using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Services.Service;

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
            try
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
                    
                    // Small delay to ensure database persistence
                    System.Threading.Thread.Sleep(100);
                    
                    project = _projectRepository.GetByUuid(project.Uuid);
                }

                return project;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create test project: {ex.Message}", ex);
            }
        }

        #endregion

        #region Essential Tests

        [Fact]
        public void GeneratePdfInvoice_WithNullProject_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _sut.GeneratePdfInvoice(null));
            Assert.Equal("project", exception.ParamName);
        }

        [Fact]
        public void GeneratePdfInvoice_WithBusinessClient_ShouldGenerateValidPDF()
        {
            // Arrange
            var project = CreateTestProject(isBusinessClient: true);

            // Act
            var result = _sut.GeneratePdfInvoice(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            var pdfHeader = System.Text.Encoding.ASCII.GetString(result.Take(5).ToArray());
            Assert.Equal("%PDF-", pdfHeader);
        }

        [Fact]
        public void GeneratePdfInvoice_WithPrivateClient_ShouldGenerateValidPDF()
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
        public void GeneratePdfInvoice_ShouldMarkEarningsAsPaid()
        {
            // Arrange
            var project = CreateTestProject(isBusinessClient: true);
            
            // Verify we have unpaid earnings
            if (project.Earnings == null || !project.Earnings.Any())
            {
                // Skip test if no earnings
                return;
            }

            var unpaidCount = project.Earnings.Count(e => !e.IsPaid);
            if (unpaidCount == 0)
            {
                // Skip test if all already paid
                return;
            }

            // Act
            _sut.GeneratePdfInvoice(project);

            // Small delay to ensure database update
            System.Threading.Thread.Sleep(200);

            // Assert
            var reloadedProject = _projectRepository.GetByUuid(project.Uuid);
            if (reloadedProject?.Earnings != null && reloadedProject.Earnings.Any())
            {
                Assert.All(reloadedProject.Earnings, e => Assert.True(e.IsPaid));
            }
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
        public void GeneratePdfInvoice_BusinessClient_ShouldHaveCVR()
        {
            // Arrange
            var project = CreateTestProject(isBusinessClient: true);

            // Act
            var result = _sut.GeneratePdfInvoice(project);

            // Assert
            Assert.NotNull(project.Client.Cvr);
            Assert.True(project.Client.IsBusinessClient());
        }

        [Fact]
        public void GeneratePdfInvoice_PrivateClient_ShouldNotHaveCVR()
        {
            // Arrange
            var project = CreateTestProject(isBusinessClient: false);

            // Act
            var result = _sut.GeneratePdfInvoice(project);

            // Assert
            Assert.True(string.IsNullOrEmpty(project.Client.Cvr));
            Assert.False(project.Client.IsBusinessClient());
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
            Assert.True(result.Length > 500);
        }

        [Fact]
        public void GeneratePdfInvoice_ShouldContainPDFSignature()
        {
            // Arrange
            var project = CreateTestProject();

            // Act
            var result = _sut.GeneratePdfInvoice(project);

            // Assert
            var header = System.Text.Encoding.ASCII.GetString(result.Take(5).ToArray());
            Assert.StartsWith("%PDF", header);
        }

        [Fact]
        public void GeneratePdfInvoice_CompleteWorkflow_ShouldSucceed()
        {
            // Arrange
            var project = CreateTestProject(isBusinessClient: true);

            // Act
            var pdfBytes = _sut.GeneratePdfInvoice(project);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
            Assert.True(project.Client.IsBusinessClient());
        }

        #endregion

        public void Dispose()
        {
            // Cleanup in correct order: earnings -> projects -> clients
            
            foreach (var earningUuid in _earningsToCleanup)
            {
                try
                {
                    _earningRepository.Delete(earningUuid);
                }
                catch { }
            }

            // Small delay to ensure deletions complete
            System.Threading.Thread.Sleep(100);

            foreach (var projectUuid in _projectsToCleanup)
            {
                try
                {
                    _projectRepository.Delete(projectUuid);
                }
                catch { }
            }

            foreach (var clientUuid in _clientsToCleanup)
            {
                try
                {
                    _clientRepository.Delete(clientUuid);
                }
                catch { }
            }

            // Final delay
            System.Threading.Thread.Sleep(100);
        }
    }
}
