using System;
using System.Windows.Input;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.Core.Model;
using System.Collections.ObjectModel;

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

        public ProjectDetailViewModel(MainViewModel mainViewModel, Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;
            _projectService = new ProjectService(); // TODO: Inject this
            SaveProjectDetailsCommand = new RelayCommand(SaveProjectDetails);
            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice);

            LoadProject();
        }

        private void LoadProject()
        {
            Project = _projectService.GetProjectByUuid(_projectId);
        }

        private void SaveProjectDetails()
        {
            
        }

        private void GenerateInvoice()
        {

        }
    }
}
