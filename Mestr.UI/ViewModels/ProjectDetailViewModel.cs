using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Data.Repository;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.UI.View;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class ProjectDetailViewModel : ViewModelBase
    {
        private readonly IEarningService _earningService;
        private readonly IExpenseService _expenseService;
        private readonly MainViewModel _mainViewModel;
        private readonly IProjectService _projectService;
        private readonly ICompanyProfileService _companyProfileService;
        private readonly PdfService _pdfService;
        private readonly Guid _projectId;
        private Project _project = null!;
        private ObservableCollection<Earning> _earnings = new();
        private ObservableCollection<Expense> _expenses = new();
        private bool _hasUnsavedChanges = false;
        private string _originalProjectName = string.Empty;
        private string _originalProjectDescription = string.Empty;
        private ProjectStatus _originalProjectStatus;
        private DateTime? _originalDeadline;
        private bool _disposed = false;
        private bool _isLoadingProject = false;
        private CompanyProfile? profile;

        public bool IsProjectCompleted => Project != null && Project.Status == ProjectStatus.Afsluttet;

        public ObservableCollection<ProjectStatus> AvailableStatuses { get; } = new ObservableCollection<ProjectStatus>
        {
            ProjectStatus.Planlagt,
            ProjectStatus.Aktiv,
            ProjectStatus.Afsluttet,
            ProjectStatus.Aflyst
        };

        public ObservableCollection<Earning> Earnings
        {
            get => _earnings;
            set
            {
                _earnings = value;
                OnPropertyChanged(nameof(Earnings));
            }
        }

        public ObservableCollection<Expense> Expenses
        {
            get => _expenses;
            set
            {
                _expenses = value;
                OnPropertyChanged(nameof(Expenses));
            }
        }

        public Project Project
        {
            get => _project;
            set
            {
                _project = value;
                OnPropertyChanged(nameof(Project));
            }
        }

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand SaveProjectDetailsCommand { get; }
        public ICommand GenerateInvoiceCommand { get; }
        public ICommand ToggleProjectStatusCommand { get; }
        public ICommand ShowEconomyWindowCommand { get; }
        public ICommand EditEarningCommand { get; }
        public ICommand EditExpenseCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand OpenCompanyInfoCommand { get; }

        public ProjectDetailViewModel(
            MainViewModel mainViewModel, 
            IProjectService projectService, 
            IEarningService earningService, 
            IExpenseService expenseService, 
            Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;

            // Initialize services with correct dependencies
            _pdfService = new PdfService();
            _companyProfileService = new CompanyProfileService();

            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _earningService = earningService ?? throw new ArgumentNullException(nameof(earningService));
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));

            NavigateToDashboardCommand = new RelayCommand(NavigateToDashboardWithWarning);
            // Initialize commands
            SaveProjectDetailsCommand = new RelayCommand(SaveProjectDetails);
            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice, CanGenerateInvoice);
            ShowEconomyWindowCommand = new RelayCommand(ShowEconomyWindow, CanAddEconomy);
            ToggleProjectStatusCommand = new RelayCommand(ToggleProjectStatus);
            EditEarningCommand = new RelayCommand<Earning>(EditEarning);
            EditExpenseCommand = new RelayCommand<Expense>(EditExpense);
            DeleteProjectCommand = new RelayCommand(DeleteProject);
            OpenCompanyInfoCommand = new RelayCommand(OpenCompanyInfo);

            LoadProject();
            profile = _companyProfileService.GetProfile();
        }

        private bool HasUnsavedChanges()
        {
            if (Project == null) return false;

            return Project.Name != _originalProjectName ||
                   Project.Description != _originalProjectDescription ||
                   Project.Status != _originalProjectStatus ||
                   Project.EndDate != _originalDeadline;
        }

        private bool ConfirmNavigationIfUnsaved()
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show(
                    "Du har ugemte ændringer. Vil du forlade siden uden at gemme?",
                    "Ugemte ændringer",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Restore original values before navigating away
                    RevertUnsavedChanges();
                    return true;
                }

                return false;

            }

            return true; // No unsaved changes, allow navigation
        }
        private void RevertUnsavedChanges()
        {
            if (Project == null) return;

            // Restore original values
            Project.Name = _originalProjectName;
            Project.Description = _originalProjectDescription;
            Project.Status = _originalProjectStatus;
            Project.EndDate = _originalDeadline;

            // Notify UI of changes
            OnPropertyChanged(nameof(Project));
        }
        private void NavigateToDashboardWithWarning()
        {
            if (!ConfirmNavigationIfUnsaved())
            {
                return; // User chose to stay on the page
            }

            // Navigate away
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }

        private void LoadProject()
        {
            var project = _projectService.GetProjectByUuid(_projectId);

            if (project == null)
            {
                MessageBox.Show(
                    "Projektet kunne ikke findes.",
                    "Fejl",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                _mainViewModel.NavigateToDashboardCommand.Execute(null);
                return;
            }

            _isLoadingProject = true;

            try
            {
                _originalProjectName = project.Name ?? string.Empty;
                _originalProjectDescription = project.Description ?? string.Empty;
                _originalProjectStatus = project.Status;
                _originalDeadline = project.EndDate;
                Project = project;
                Earnings = project.Earnings != null
                    ? new ObservableCollection<Earning>(project.Earnings)
                    : new ObservableCollection<Earning>();
                Expenses = project.Expenses != null
                    ? new ObservableCollection<Expense>(project.Expenses)
                    : new ObservableCollection<Expense>();
                _hasUnsavedChanges = false;
            }
            finally
            {
                _isLoadingProject = false;
            }
        }

        private void SaveProjectDetails()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            try
            {
                // Sync collections back to Project before saving
                Project.Earnings = Earnings.ToList();
                Project.Expenses = Expenses.ToList();

                _projectService.UpdateProject(Project);
                _originalProjectName = Project.Name ?? string.Empty;
                _originalProjectDescription = Project.Description ?? string.Empty;
                _originalProjectStatus = Project.Status;
                _originalDeadline = Project.EndDate;
                _hasUnsavedChanges = false;

                MessageBox.Show(
                    "Dine ændringer er nu gemt",
                    "Gem succesfuldt",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Kunne ikke gemme projektet. Fejl: {ex.Message}",
                    "Gem mislykkedes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool CanGenerateInvoice()
        {
            var unpaidEarnings = Project.Earnings?.Where(e => !e.IsPaid).ToList() ?? new List<Earning>();
            return unpaidEarnings.Count > 0;
        }

        private void GenerateInvoice()
        {
            if (Project == null)
                return;

            var dialog = new SaveFileDialog
            {
                FileName = $"Invoice_{Project.Name}.pdf",
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            string filePath = dialog.FileName;

            try
            {
                // Generer PDF som byte-array
                var pdfBytes = _pdfService.GeneratePdfInvoice(Project);

                // Gem filen synkront
                File.WriteAllBytes(filePath, pdfBytes);

                // Åbn filen
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fejl ved generering af PDF: {ex.Message}");
            }
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }

        private void ShowEconomyWindow()
        {
            // RETTELSE: Brug korrekt constructor med services først
            var economyVm = new EconomyViewModel(
                _projectId,
                _earningService,
                _expenseService);

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            LoadProject();
        }

        private bool CanAddEconomy()
        {
            return Project.Status != ProjectStatus.Afsluttet;
        }

        private void EditEarning(Earning? earning)
        {
            if (earning == null) return;

            // RETTELSE: Brug constructor med Earning parameter
            var economyVm = new EconomyViewModel(
                projectUuid: _projectId,
                earningService: _earningService,
                expenseService: _expenseService,
                earningToEdit: earning);

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            LoadProject();
        }

        private void EditExpense(Expense? expense)
        {
            if (expense == null) return;

            // RETTELSE: Brug constructor med Expense parameter
            var economyVm = new EconomyViewModel(
                projectUuid: _projectId,
                earningService: _earningService,
                expenseService: _expenseService,
                expenseToEdit: expense);

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            LoadProject();
        }

        private void ToggleProjectStatus()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            if (IsProjectCompleted)
            {
                Project.Status = ProjectStatus.Aktiv;
                _projectService.UpdateProjectStatus(_projectId, ProjectStatus.Aktiv);
            }
            else
            {
                Project.Status = ProjectStatus.Afsluttet;
                _projectService.CompleteProject(_projectId);
            }

            OnPropertyChanged(nameof(IsProjectCompleted));
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }

        private void DeleteProject()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            var result = MessageBox.Show(
                $"Er du sikker på, at du vil slette projektet '{Project.Name}'?",
                "Bekræft sletning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _projectService.DeleteProject(_projectId);
                    _mainViewModel.NavigateToDashboardCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Kunne ikke slette projektet. Fejl: {ex.Message}",
                        "Sletning mislykkedes",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OpenCompanyInfo()
        {
            // Hent den nyeste version fra databasen for at sikre, vi har de seneste data
            var currentProfile = _companyProfileService.GetProfile();

            if (currentProfile != null)
            {
                // Brug den hentede profil
                var addCompanyInfoVm = new AddCompanyInfoViewModel(_companyProfileService, currentProfile);

                var addCompanyInfoWindow = new AddCompanyInfoWindow()
                {
                    DataContext = addCompanyInfoVm,
                    Owner = App.Current.MainWindow,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
                };

                addCompanyInfoWindow.ShowDialog();

                // Opdater den lokale profil-variabel
                profile = _companyProfileService.GetProfile();
            }
        }
    }
}