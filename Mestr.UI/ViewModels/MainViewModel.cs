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
        
        public ViewModelBase CurrentViewModel 
        { 
            get => _currentViewModel!;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public ICommand NavigateToProjectCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToProjectDetailsCommand { get; }
        public ICommand NavigateToClientsCommand { get; }

        public MainViewModel()
        {
            
            // Initialize services with dependencies
            _projectService = new ProjectService();
            
            // Non-parameterized navigation
            NavigateToProjectCommand = new RelayCommand(NavigateToProject);
            NavigateToDashboardCommand = new RelayCommand(NavigateToDashboard);
            NavigateToClientsCommand = new RelayCommand(NavigateToClients);

            // Parameterized navigation - expects Guid
            NavigateToProjectDetailsCommand = new RelayCommand<Guid>(NavigateToProjectDetails);
            
            // Set initial ViewModel
            CurrentViewModel = new DashboardViewModel(this, _projectService);
        }

        private void NavigateToProject()
        {
            CurrentViewModel = new ProjectViewModel(this, _projectService);
        }

        private void NavigateToDashboard()
        {
            CurrentViewModel = new DashboardViewModel(this, _projectService);
        }

        private void NavigateToClients()
        {
            CurrentViewModel = new ClientViewModel(this, new ClientService());
        }

        private void NavigateToProjectDetails(Guid projectUuid)
        {
            CurrentViewModel = new ProjectDetailViewModel(this, projectUuid);
        }
    }
}
