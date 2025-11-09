using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mestr.UI.Command;
using Mestr.Services.Interface;
using Mestr.Services.Service;

namespace Mestr.UI.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IProjectService _projectService;
        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }

            set
            {
                _projectName = value;
                OnPropertyChanged(nameof(ProjectName));
            }
        }


        private string _clientName;
        public string ClientName
        {
            get { return _clientName; }
            set
            {
                _clientName = value;
                OnPropertyChanged(nameof(ClientName));
            }
        }


        private DateTime _deadline;
        public DateTime Deadline
        {
            get { return _deadline; }
            set
            {
                _deadline = value;
                OnPropertyChanged(nameof(Deadline));
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }

        }

        public ICommand CreateProjectCommand { get; }
        public ICommand NavigateToDashboardCommand => _mainViewModel?.NavigateToDashboardCommand;

        public ProjectViewModel(MainViewModel mainViewModel, IProjectService projectService)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            CreateProjectCommand = new RelayCommand(CreateProject);
        }

        private void CreateProject()
        {
            var project = _projectService.CreateProject(ProjectName, Description, Deadline);
            
            // Option 1: Navigate to dashboard
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
            
            // Option 2: Navigate to the newly created project details
            // _mainViewModel.NavigateToProjectDetailsCommand.Execute(project.Uuid);
        }

    }

}