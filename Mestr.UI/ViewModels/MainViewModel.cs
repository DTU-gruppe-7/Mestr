using Mestr.UI.View;
using Mestr.UI.Command;
using Mestr.Services.Service;
using Mestr.Services.Interface;
using Mestr.Data.Repository;
using System;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentViewModel;
        private readonly IProjectService _projectService;
        private readonly IClientService _clientService;

        public ViewModelBase CurrentViewModel 
        { 
            get => _currentViewModel!;
            set
            {
                if (_currentViewModel is IDisposable disposable)
                { 
                    disposable.Dispose();
                }
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public ICommand NavigateToAddNewProjectCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToProjectDetailsCommand { get; }
        public ICommand NavigateToClientsCommand { get; }

        public MainViewModel()
        {
            
            // Initialize services with dependencies
            _projectService = new ProjectService();
            _clientService = new ClientService();

            // Non-parameterized navigation
            NavigateToAddNewProjectCommand = new RelayCommand(NavigateToAddNewProject);
            NavigateToDashboardCommand = new RelayCommand(NavigateToDashboard);
            NavigateToClientsCommand = new RelayCommand(NavigateToClients);

            // Parameterized navigation - expects Guid
            NavigateToProjectDetailsCommand = new RelayCommand<Guid>(NavigateToProjectDetails);
            
            // Set initial ViewModel
            CurrentViewModel = new DashboardViewModel(this, _projectService);
        }

        private void NavigateToAddNewProject()
        {
            CurrentViewModel = new AddNewProjectViewModel(this, _projectService, _clientService);
        }

        private void NavigateToDashboard()
        {
            CurrentViewModel = new DashboardViewModel(this, _projectService);
        }

        private void NavigateToClients()
        {
            CurrentViewModel = new ClientViewModel(this, _clientService);
        }

        private void NavigateToProjectDetails(Guid projectUuid)
        {
            CurrentViewModel = new ProjectDetailViewModel(this, projectUuid);
        }
    }
}
