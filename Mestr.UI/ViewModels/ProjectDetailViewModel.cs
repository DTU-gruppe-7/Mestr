using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.UI.View;
using System;
using System.Collections.ObjectModel;
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
                //kalder profitLoss når en ny indtægt indsættes og udregner balancen
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
                //kalder profitLoss når en ny udgift indsættes og udregner balancen
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

        public ProjectDetailViewModel(MainViewModel mainViewModel, Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;
            _projectService = new ProjectService(); // TODO: Inject this
            _earningService = new EarningService();
            _expenseService = new ExpenseService();

            SaveProjectDetailsCommand = new RelayCommand(SaveProjectDetails);
            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice);

            ShowEconomyWindowCommand = new RelayCommand(ShowEconomyWindow);

            ToggleProjectStatusCommand = new RelayCommand(ToggleProjectStatus);

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

                // Initialize Earnings and Expenses collection
                Earnings = new ObservableCollection<Earning>(earningsList);
                Expenses = new ObservableCollection<Expense>(expensesList);
            }
            else
            {
                // Handle the case where the project is not found (optional: set to a default or throw)
                // Project = new Project(); // if you want to avoid nulls entirely
            }
        }


        private void SaveProjectDetails()
        {
            //Todo: Implement save logic

            //Update the project in the data store
            if (Project != null)
            {
                _projectService.UpdateProject(Project);

            }

            _mainViewModel.NavigateToDashboardCommand.Execute(null);


        }

        private void GenerateInvoice()
        {
            //ToDo Implement Invoice logic

            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }


        private void ShowEconomyWindow()
        {
            var economyVm = new EconomyViewModel(
                _projectId,
                _earningService,  // Inject services
                _expenseService);

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm, // VIGTIGT: Ellers er vinduet tomt!
                Owner = App.Current.MainWindow
            };

            // 2. Vi viser vinduet og venter
            economyWindow.ShowDialog();

            // 3. VIGTIGT: Når vinduet lukkes, skal vi hente data igen!
            LoadProject();
        }

        private void CompleteProject()
        {
            if (Project != null)
            {
                _projectService.CompleteProject(_projectId);
                _mainViewModel.NavigateToDashboardCommand.Execute(null);
            }

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
                // Start with zero
                decimal totalIncome = 0;
                decimal totalExpense = 0;

                // Check for null values
                if (Earnings != null)
                    totalIncome = Earnings.Sum(e => e.Amount);

                if (Expenses != null)
                    totalExpense = Expenses.Sum(e => e.Amount);

                // Result
                return totalIncome - totalExpense;
            }

        }
    }
}
