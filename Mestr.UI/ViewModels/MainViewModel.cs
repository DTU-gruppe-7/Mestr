using Mestr.UI.View;
using Mestr.UI.Command;
using Mestr.Services.Service;
using System;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        
        public ViewModelBase CurrentViewModel 
        { 
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public ICommand NavigateToProjectCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToProjectDetailsCommand { get; }

        public MainViewModel()
        {
            // Non-parameterized navigation
            NavigateToProjectCommand = new RelayCommand(NavigateToProject);
            NavigateToDashboardCommand = new RelayCommand(NavigateToDashboard);

            // Parameterized navigation - expects Guid
            NavigateToProjectDetailsCommand = new RelayCommand<Guid>(NavigateToProjectDetails);
            
            // Set initial ViewModel
            CurrentViewModel = new DashboardViewModel(this, new ProjectService());
        }

        private void NavigateToProject()
        {
            CurrentViewModel = new ProjectViewModel(this, new ProjectService());
        }

        private void NavigateToDashboard()
        {
            CurrentViewModel = new DashboardViewModel(this, new ProjectService());
        }

        private void NavigateToProjectDetails(Guid projectUuid)
        {
            CurrentViewModel = new ProjectDetailViewModel(this, projectUuid);
        }

        
    }
}
