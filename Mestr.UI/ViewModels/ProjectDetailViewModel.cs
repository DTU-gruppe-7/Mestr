using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.Core.Model;
using System.Collections.ObjectModel;
using Mestr.UI.View;
using System;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class ProjectDetailViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IProjectService _projectService;
        private readonly Guid _projectId;
        private Project _project;
        private ObservableCollection<Earning> _earnings;
        private ObservableCollection<Expense> _expenses;

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

        public ICommand NavigateToDashboardCommand => _mainViewModel?.NavigateToDashboardCommand;

        public ICommand SaveProjectDetailsCommand { get; }
        public ICommand GenerateInvoiceCommand { get; }
        public ICommand CompleteProjectCommand { get; }
        public ICommand ShowEconomyWindowCommand { get; }

        public ProjectDetailViewModel(MainViewModel mainViewModel, Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;
            _projectService = new ProjectService(); // TODO: Inject this
            
            SaveProjectDetailsCommand = new RelayCommand(SaveProjectDetails);
            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice);
           
            ShowEconomyWindowCommand = new RelayCommand(ShowEconomyWindow);

            CompleteProjectCommand = new RelayCommand(CompleteProject);

            LoadProject();
        }

        private void LoadProject()
        {
            Project = _projectService.GetProjectByUuid(_projectId);
        }


        private void SaveProjectDetails()
        {
            //Todo: Implement save logic

            _mainViewModel.NavigateToDashboardCommand.Execute(null);

        }

        private void GenerateInvoice()
        {
            //ToDo Implement Invoice logic
            
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }


        private void ShowEconomyWindow()
        {
            var economyWindow = new EconomyWindow();
            economyWindow.Owner = App.Current.MainWindow;
            economyWindow.ShowDialog();
        }
        
        private void CompleteProject()
        {
            if (Project != null)
            {
                _projectService.CompleteProject(_projectId);
                _mainViewModel.NavigateToDashboardCommand.Execute(null);
            }

        }
    }
}
