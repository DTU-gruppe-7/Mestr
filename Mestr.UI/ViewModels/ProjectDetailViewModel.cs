using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.Data.Repository;
using Mestr.UI.Command;
using Mestr.UI.View;
using System;
using System.Collections.ObjectModel;
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
        private readonly Guid _projectId;
        private Project _project = null!;
        private ObservableCollection<Earning> _earnings = new();
        private ObservableCollection<Expense> _expenses = new();

        public bool IsProjectCompleted => Project != null && Project.Status == ProjectStatus.Afsluttet;

        public ObservableCollection<Earning> Earnings
        {
            get => _earnings;   
            set
            {
                _earnings = value;
                OnPropertyChanged(nameof(Earnings));
                OnPropertyChanged(nameof(ProfitLoss));
            }
        }

        public ObservableCollection<Expense> Expenses
        {
            get => _expenses;
            set
            {
                _expenses = value;
                OnPropertyChanged(nameof(Expenses));
                OnPropertyChanged(nameof(ProfitLoss));
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

        public ICommand NavigateToDashboardCommand => _mainViewModel.NavigateToDashboardCommand;
        public ICommand SaveProjectDetailsCommand { get; }
        public ICommand GenerateInvoiceCommand { get; }
        public ICommand ToggleProjectStatusCommand { get; }
        public ICommand ShowEconomyWindowCommand { get; }
        public ICommand EditEarningCommand { get; }
        public ICommand EditExpenseCommand { get; }
        public ICommand DeleteProjectCommand { get; }

        public ProjectDetailViewModel(MainViewModel mainViewModel, Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;
                
            
            // Initialize services with correct dependencies
            _projectService = new ProjectService();
            _earningService = new EarningService();
            _expenseService = new ExpenseService();

            // Initialize commands
            SaveProjectDetailsCommand = new RelayCommand(SaveProjectDetails);
            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice);
            ShowEconomyWindowCommand = new RelayCommand(ShowEconomyWindow);
            ToggleProjectStatusCommand = new RelayCommand(ToggleProjectStatus);
            EditEarningCommand = new RelayCommand<Earning>(EditEarning);
            EditExpenseCommand = new RelayCommand<Expense>(EditExpense);
            DeleteProjectCommand = new RelayCommand(DeleteProject);

            LoadProject();
        }

        private void LoadProject()
        {
            var project = _projectService.GetProjectByUuid(_projectId);

            if (project != null)
            {
                Project = project;
                Earnings = project.Earnings != null
                    ? new ObservableCollection<Earning>(project.Earnings)
                    : new ObservableCollection<Earning>();
                Expenses = project.Expenses != null
                    ? new ObservableCollection<Expense>(project.Expenses)
                    : new ObservableCollection<Expense>();
            }
        }

        private void SaveProjectDetails()
        {
            if (Project != null)
            {
                // Sync collections back to Project before saving
                Project.Earnings = Earnings.ToList();
                Project.Expenses = Expenses.ToList();
                
                _projectService.UpdateProject(Project);
            }
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }

        private void GenerateInvoice()
        {
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

        public decimal ProfitLoss
        {
            get
            {
                decimal totalIncome = Earnings?.Sum(e => e.Amount) ?? 0;
                decimal totalExpense = Expenses?.Sum(e => e.Amount) ?? 0;
                return totalIncome - totalExpense;
            }
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
    }
}
