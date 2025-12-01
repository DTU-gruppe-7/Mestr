using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Data.Repository;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.UI.Utilities;
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
        
        // Track items that need to be added to database
        private readonly List<Earning> _pendingEarningsToAdd = new();
        private readonly List<Expense> _pendingExpensesToAdd = new();
        
        // Track items that need to be updated in database
        private readonly List<Earning> _pendingEarningsToUpdate = new();
        private readonly List<Expense> _pendingExpensesToUpdate = new();

        // Track items that need to be deleted from database
        private readonly List<Earning> _pendingEarningsToDelete = new();
        private readonly List<Expense> _pendingExpensesToDelete = new();

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
                   Project.EndDate != _originalDeadline ||
                   _pendingEarningsToAdd.Count > 0 ||
                   _pendingExpensesToAdd.Count > 0 ||
                   _pendingEarningsToUpdate.Count > 0 ||
                   _pendingExpensesToUpdate.Count > 0 ||
                   _pendingEarningsToDelete.Count > 0 ||
                   _pendingExpensesToDelete.Count > 0;
        }

        private bool ConfirmNavigationIfUnsaved()
        {
            if (HasUnsavedChanges())
            {
                if (MessageBoxHelper.Standard.ConfirmUnsavedChanges())
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

            // Restore original project values
            Project.Name = _originalProjectName;
            Project.Description = _originalProjectDescription;
            Project.Status = _originalProjectStatus;
            Project.EndDate = _originalDeadline;

            // Restore deleted items back to UI
            foreach (var earning in _pendingEarningsToDelete)
            {
                if (!Earnings.Contains(earning))
                {
                    Earnings.Add(earning);
                }
            }
            foreach (var expense in _pendingExpensesToDelete)
            {
                if (!Expenses.Contains(expense))
                {
                    Expenses.Add(expense);
                }
            }
            
            // Remove pending additions from UI
            foreach (var earning in _pendingEarningsToAdd.ToList())
            {
                Earnings.Remove(earning);
            }
            foreach (var expense in _pendingExpensesToAdd.ToList())
            {
                Expenses.Remove(expense);
            }
            
            // Revert pending updates by reloading from database
            LoadProject();
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

        private async void LoadProject()
        {
            var project = await _projectService.GetProjectByUuidAsync(_projectId);

            if (project == null)
            {
                MessageBoxHelper.Standard.ProjectNotFound();
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
                
                // Clear pending changes
                _pendingEarningsToAdd.Clear();
                _pendingExpensesToAdd.Clear();
                _pendingEarningsToUpdate.Clear();
                _pendingExpensesToUpdate.Clear();
                _pendingEarningsToDelete.Clear();
                _pendingExpensesToDelete.Clear();
                
                // Update command states after loading project
                ((RelayCommand)GenerateInvoiceCommand).RaiseCanExecuteChanged();
            }
            finally
            {
                _isLoadingProject = false;
            }
        }

        private async void SaveProjectDetails()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            try
            {
                // Process pending deletions first
                foreach (var earning in _pendingEarningsToDelete.ToList())
                {
                    try
                    {
                        await _earningService.DeleteAsync(earning);
                    }
                    catch (Exception ex)
                    {
                        // Log but continue - item might already be deleted
                        System.Diagnostics.Debug.WriteLine($"Could not delete earning {earning.Uuid}: {ex.Message}");
                    }
                }
                foreach (var expense in _pendingExpensesToDelete.ToList())
                {
                    try
                    {
                        await _expenseService.DeleteAsync(expense);
                    }
                    catch (Exception ex)
                    {
                        // Log but continue - item might already be deleted
                        System.Diagnostics.Debug.WriteLine($"Could not delete expense {expense.Uuid}: {ex.Message}");
                    }
                }
                
                // Process pending additions
                foreach (var earning in _pendingEarningsToAdd.ToList())
                {
                    try
                    {
                        await _earningService.AddNewEarningAsync(earning.ProjectUuid, earning.Description, earning.Amount, earning.Date);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxHelper.Standard.AddTransactionError($"indtægt '{earning.Description}'", ex.Message);
                    }
                }
                foreach (var expense in _pendingExpensesToAdd.ToList())
                {
                    try
                    {
                        await _expenseService.AddNewExpenseAsync(expense.ProjectUuid, expense.Description, expense.Amount, expense.Date, expense.Category);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxHelper.Standard.AddTransactionError($"udgift '{expense.Description}'", ex.Message);
                    }
                }
                
                // Process pending updates
                foreach (var earning in _pendingEarningsToUpdate.ToList())
                {
                    try
                    {
                        await _earningService.UpdateAsync(earning);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxHelper.Standard.UpdateTransactionError($"indtægt '{earning.Description}'", ex.Message);
                    }
                }
                foreach (var expense in _pendingExpensesToUpdate.ToList())
                {
                    try
                    {
                        await _expenseService.UpdateAsync(expense);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxHelper.Standard.UpdateTransactionError($"udgift '{expense.Description}'", ex.Message);
                    }
                }
                
                // Reload project from database to get fresh state
                var freshProject = await _projectService.GetProjectByUuidAsync(_projectId);
                if (freshProject != null)
                {
                    // Update project details (name, description, status, etc.)
                    freshProject.Name = Project.Name;
                    freshProject.Description = Project.Description;
                    freshProject.Status = Project.Status;
                    freshProject.EndDate = Project.EndDate;
                    
                    await _projectService.UpdateProjectAsync(freshProject);
                    
                    // Reload the entire project to ensure we have the latest state
                    LoadProject();
                }
                
                MessageBoxHelper.Standard.ProjectUpdated();

            }
            catch (Exception ex)
            {
                MessageBoxHelper.Standard.SaveError(ex.Message);
            }
        }

        private bool CanGenerateInvoice()
        {
            var unpaidEarnings = Project.Earnings?.Where(e => !e.IsPaid).ToList() ?? new List<Earning>();
            return unpaidEarnings.Count > 0;
        }

        private async void GenerateInvoice()
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
                var pdfBytes = await _pdfService.GeneratePdfInvoiceAsync(Project);

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
                MessageBoxHelper.Standard.PdfGenerationError(ex.Message);
            }
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }

        private void ShowEconomyWindow()
        {
            var economyVm = new EconomyViewModel(
                _projectId,
                _earningService,
                _expenseService,
                onEarningSaved: (earning) =>
                {
                    // Check if this is a new earning or an update
                    var existingEarning = Earnings.FirstOrDefault(e => e.Uuid == earning.Uuid);
                    if (existingEarning != null)
                    {
                        // Update existing earning in observable collection
                        var index = Earnings.IndexOf(existingEarning);
                        if (index >= 0)
                        {
                            Earnings.RemoveAt(index);
                            Earnings.Insert(index, earning);
                        }
                        
                        // Track for database update
                        _pendingEarningsToUpdate.RemoveAll(e => e.Uuid == earning.Uuid);
                        _pendingEarningsToUpdate.Add(earning);
                    }
                    else
                    {
                        // Add new earning to observable collection
                        Earnings.Add(earning);
                        
                        // Track for database addition
                        _pendingEarningsToAdd.Add(earning);
                    }
                    
                    // Sync to Project immediately for UI updates
                    Project.Earnings = Earnings.ToList();
                    
                    OnPropertyChanged(nameof(Project));
                    OnPropertyChanged(nameof(Earnings));
                },
                onExpenseSaved: (expense) =>
                {
                    // Check if this is a new expense or an update
                    var existingExpense = Expenses.FirstOrDefault(e => e.Uuid == expense.Uuid);
                    if (existingExpense != null)
                    {
                        // Update existing expense in observable collection
                        var index = Expenses.IndexOf(existingExpense);
                        if (index >= 0)
                        {
                            Expenses.RemoveAt(index);
                            Expenses.Insert(index, expense);
                        }
                        
                        // Track for database update
                        _pendingExpensesToUpdate.RemoveAll(e => e.Uuid == expense.Uuid);
                        _pendingExpensesToUpdate.Add(expense);
                    }
                    else
                    {
                        // Add new expense to observable collection
                        Expenses.Add(expense);
                        
                        // Track for database addition
                        _pendingExpensesToAdd.Add(expense);
                    }
                    
                    // Sync to Project immediately for UI updates
                    Project.Expenses = Expenses.ToList();
                    
                    OnPropertyChanged(nameof(Project));
                    OnPropertyChanged(nameof(Expenses));
                });

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            
            // Update command state after economy window closes
            ((RelayCommand)GenerateInvoiceCommand).RaiseCanExecuteChanged();
        }

        private bool CanAddEconomy()
        {
            return Project.Status != ProjectStatus.Afsluttet;
        }

        private void EditEarning(Earning? earning)
        {
            if (earning == null) return;

            // Store the Guid for tracking
            var earningGuid = earning.Uuid;

            var economyVm = new EconomyViewModel(
                projectUuid: _projectId,
                earningService: _earningService,
                expenseService: _expenseService,
                earningToEdit: earning,
                onEarningSaved: (updatedEarning) =>
                {
                    // Find and update by Guid instead of reference
                    var existingIndex = -1;
                    for (int i = 0; i < Earnings.Count; i++)
                    {
                        if (Earnings[i].Uuid == earningGuid)
                        {
                            existingIndex = i;
                            break;
                        }
                    }
                    
                    if (existingIndex >= 0)
                    {
                        Earnings.RemoveAt(existingIndex);
                        Earnings.Insert(existingIndex, updatedEarning);
                    }
                    
                    // Sync to Project immediately for UI updates
                    Project.Earnings = Earnings.ToList();
                    
                    // Track for database update (check by Guid)
                    var wasNewItem = _pendingEarningsToAdd.Any(e => e.Uuid == earningGuid);
                    
                    if (!wasNewItem)
                    {
                        // Remove old reference and add new one
                        _pendingEarningsToUpdate.RemoveAll(e => e.Uuid == earningGuid);
                        _pendingEarningsToUpdate.Add(updatedEarning);
                    }
                    else
                    {
                        // Update the reference in the pending add list
                        _pendingEarningsToAdd.RemoveAll(e => e.Uuid == earningGuid);
                        _pendingEarningsToAdd.Add(updatedEarning);
                    }
                    
                    OnPropertyChanged(nameof(Project));
                    OnPropertyChanged(nameof(Earnings));
                },
                onEarningDeleted: (deletedEarning) =>
                {
                    // Remove from observable collection (UI update)
                    var toRemove = Earnings.FirstOrDefault(e => e.Uuid == deletedEarning.Uuid);
                    if (toRemove != null)
                    {
                        Earnings.Remove(toRemove);
                    }
                    
                    // Sync to Project immediately for UI updates
                    Project.Earnings = Earnings.ToList();
                    
                    // Track for database deletion (check by Guid)
                    var wasNewItem = _pendingEarningsToAdd.Any(e => e.Uuid == deletedEarning.Uuid);
                    
                    if (wasNewItem)
                    {
                        // If it was pending addition, just remove it from that list (never saved to DB)
                        _pendingEarningsToAdd.RemoveAll(e => e.Uuid == deletedEarning.Uuid);
                    }
                    else
                    {
                        // Otherwise, mark for deletion (will be deleted on Save)
                        if (!_pendingEarningsToDelete.Any(e => e.Uuid == deletedEarning.Uuid))
                        {
                            _pendingEarningsToDelete.Add(deletedEarning);
                        }
                    }
                    
                    // Remove from update list if present
                    _pendingEarningsToUpdate.RemoveAll(e => e.Uuid == deletedEarning.Uuid);
                    
                    OnPropertyChanged(nameof(Project));
                    OnPropertyChanged(nameof(Earnings));
                });

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            
            // Update command state after editing earning
            ((RelayCommand)GenerateInvoiceCommand).RaiseCanExecuteChanged();
        }

        private void EditExpense(Expense? expense)
        {
            if (expense == null) return;

            // Store the Guid for tracking
            var expenseGuid = expense.Uuid;

            var economyVm = new EconomyViewModel(
                projectUuid: _projectId,
                earningService: _earningService,
                expenseService: _expenseService,
                expenseToEdit: expense,
                onExpenseSaved: (updatedExpense) =>
                {
                    // Find and update by Guid instead of reference
                    var existingIndex = -1;
                    for (int i = 0; i < Expenses.Count; i++)
                    {
                        if (Expenses[i].Uuid == expenseGuid)
                        {
                            existingIndex = i;
                            break;
                        }
                    }
                    
                    if (existingIndex >= 0)
                    {
                        Expenses.RemoveAt(existingIndex);
                        Expenses.Insert(existingIndex, updatedExpense);
                    }
                    
                    // Sync to Project immediately for UI updates
                    Project.Expenses = Expenses.ToList();
                    
                    // Track for database update (check by Guid)
                    var wasNewItem = _pendingExpensesToAdd.Any(e => e.Uuid == expenseGuid);
                    
                    if (!wasNewItem)
                    {
                        // Remove old reference and add new one
                        _pendingExpensesToUpdate.RemoveAll(e => e.Uuid == expenseGuid);
                        _pendingExpensesToUpdate.Add(updatedExpense);
                    }
                    else
                    {
                        // Update the reference in the pending add list
                        _pendingExpensesToAdd.RemoveAll(e => e.Uuid == expenseGuid);
                        _pendingExpensesToAdd.Add(updatedExpense);
                    }
                    
                    OnPropertyChanged(nameof(Project));
                    OnPropertyChanged(nameof(Expenses));
                },
                onExpenseDeleted: (deletedExpense) =>
                {
                    // Remove from observable collection (UI update)
                    var toRemove = Expenses.FirstOrDefault(e => e.Uuid == deletedExpense.Uuid);
                    if (toRemove != null)
                    {
                        Expenses.Remove(toRemove);
                    }
                    
                    // Sync to Project immediately for UI updates
                    Project.Expenses = Expenses.ToList();
                    
                    // Track for database deletion (check by Guid)
                    var wasNewItem = _pendingExpensesToAdd.Any(e => e.Uuid == deletedExpense.Uuid);
                    
                    if (wasNewItem)
                    {
                        // If it was pending addition, just remove it from that list (never saved to DB)
                        _pendingExpensesToAdd.RemoveAll(e => e.Uuid == deletedExpense.Uuid);
                    }
                    else
                    {
                        // Otherwise, mark for deletion (will be deleted on Save)
                        if (!_pendingExpensesToDelete.Any(e => e.Uuid == deletedExpense.Uuid))
                        {
                            _pendingExpensesToDelete.Add(deletedExpense);
                        }
                    }
                    
                    // Remove from update list if present
                    _pendingExpensesToUpdate.RemoveAll(e => e.Uuid == deletedExpense.Uuid);
                    
                    OnPropertyChanged(nameof(Project));
                    OnPropertyChanged(nameof(Expenses));
                });

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            
            // Update command state after editing expense
            ((RelayCommand)GenerateInvoiceCommand).RaiseCanExecuteChanged();
        }

        private async void ToggleProjectStatus()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            if (IsProjectCompleted)
            {
                Project.Status = ProjectStatus.Aktiv;
                await _projectService.UpdateProjectStatusAsync(_projectId, ProjectStatus.Aktiv);
            }
            else
            {
                Project.Status = ProjectStatus.Afsluttet;
                await _projectService.CompleteProjectAsync(_projectId);
            }

            OnPropertyChanged(nameof(IsProjectCompleted));
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }

        private async void DeleteProject()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            if (!MessageBoxHelper.Standard.ConfirmDeleteProject(Project.Name))
                return;

            try
            {
                await _projectService.DeleteProjectAsync(_projectId);
                _mainViewModel.NavigateToDashboardCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Standard.DeleteError(ex.Message);
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