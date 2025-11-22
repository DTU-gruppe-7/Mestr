using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class AddNewProjectViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IProjectService _projectService;
        private string _projectName = string.Empty;

        public ICommand CreateProjectCommand { get; }
        public ICommand NavigateToDashboardCommand => _mainViewModel.NavigateToDashboardCommand;

        public AddNewProjectViewModel(MainViewModel mainViewModel, IProjectService projectService)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            CreateProjectCommand = new RelayCommand(CreateProject, CanCreateProject);
        }
        public string ProjectName
        {
            get { return _projectName; }

            set
            {
                _projectName = value;
                OnPropertyChanged(nameof(ProjectName));
                ValidateText(nameof(ProjectName), value, "Projektnavn");
                ((RelayCommand)CreateProjectCommand).RaiseCanExecuteChanged();
            }
        }


        private string _clientName = string.Empty;
        public string ClientName
        {
            get { return _clientName; }
            set
            {
                _clientName = value;
                OnPropertyChanged(nameof(ClientName));
                ValidateText(nameof(ClientName), value, "Kundenavn");
                ((RelayCommand)CreateProjectCommand).RaiseCanExecuteChanged();
            }
        }


        private DateTime _deadline = DateTime.Now;
        public DateTime Deadline
        {
            get { return _deadline; }
            set
            {
                _deadline = value;
                OnPropertyChanged(nameof(Deadline));
                ValidateDate(nameof(Deadline), value);
                ((RelayCommand)CreateProjectCommand).RaiseCanExecuteChanged();
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }

        }

        

        private void CreateProject()
        {
            var project = _projectService.CreateProject(ProjectName, ClientName, Description, Deadline);
            
            // Option 1: Navigate to dashboard
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
            
            // Option 2: Navigate to the newly created project details
            // _mainViewModel.NavigateToProjectDetailsCommand.Execute(project.Uuid);
        }
        private bool CanCreateProject()
        {
            return !HasErrors
                && !string.IsNullOrWhiteSpace(ProjectName)
                && !string.IsNullOrWhiteSpace(ClientName);
        }

    }
}

    
