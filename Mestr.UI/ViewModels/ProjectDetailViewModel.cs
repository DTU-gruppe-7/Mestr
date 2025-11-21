using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.UI.View;
using System;
using System.Collections.ObjectModel    ;
using System.Linq;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class ProjectDetailViewModel : ViewModelBase
    {
        private readonly EarningService _earningService;
        private readonly ExpenseService _expenseService;
        private readonly MainViewModel _mainViewModel;
        private readonly ProjectService _projectService;
        private readonly Guid _projectId;
        private Project _project = null!;
        private ObservableCollection<Earning> _earnings;
        private ObservableCollection<Expense> _expenses;

        public bool IsProjectCompleted => Project != null && Project.Status == ProjectStatus.Completed;

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
        
        // ? TILFØJ DISSE TO NYE COMMANDS
        public ICommand EditEarningCommand { get; }
        public ICommand EditExpenseCommand { get; }

        public ProjectDetailViewModel(MainViewModel mainViewModel, Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;
            _projectService = new ProjectService();
            _earningService = new EarningService();
            _expenseService = new ExpenseService();

            SaveProjectDetailsCommand = new RelayCommand(SaveProjectDetails);
            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice);
            ShowEconomyWindowCommand = new RelayCommand(ShowEconomyWindow);
            ToggleProjectStatusCommand = new RelayCommand(ToggleProjectStatus);
            
            // ? INITIALISER DE NYE COMMANDS
            EditEarningCommand = new RelayCommand<Earning>(EditEarning);
            EditExpenseCommand = new RelayCommand<Expense>(EditExpense);

            LoadProject();
        }

        private void LoadProject()
        {
            var project = _projectService.GetProjectByUuid(_projectId);
            var earningsList = _earningService.GetAllByProjectUuid(_projectId);
            var expensesList = _expenseService.GetAllByProjectUuid(_projectId);

            if (project != null)
            {
                Project = project;
                Earnings = new ObservableCollection<Earning>(earningsList);
                Expenses = new ObservableCollection<Expense>(expensesList);
            }
        }

        private void SaveProjectDetails()
        {
            if (Project != null)
            {
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

            var economyVm = new EconomyViewModel(
                _projectId,
                _earningService,
                _expenseService,
                earning); // Pass earning til edit constructor

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            LoadProject(); // Refresh data efter lukket dialog
        }

        private void EditExpense(Expense? expense)
        {
            if (expense == null) return;

            var economyVm = new EconomyViewModel(
                _projectId,
                _earningService,
                _expenseService,
                expense); // Pass expense til edit constructor

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            LoadProject(); // Refresh data efter lukket dialog
        }

        private void ToggleProjectStatus()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            if (IsProjectCompleted)
            {
                Project.Status = ProjectStatus.Ongoing;
                _projectService.UpdateProjectStatus(_projectId, ProjectStatus.Ongoing);
            }
            else
            {
                Project.Status = ProjectStatus.Completed;
                _projectService.CompleteProject(_projectId);
            }

            OnPropertyChanged(nameof(IsProjectCompleted));
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }

        public decimal ProfitLoss
        {
            get
            {
                decimal totalIncome = 0;
                decimal totalExpense = 0;

                if (Earnings != null)
                    totalIncome = Earnings.Sum(e => e.Amount);

                if (Expenses != null)
                    totalExpense = Expenses.Sum(e => e.Amount);

                return totalIncome - totalExpense;
            }
        }
    }
}
