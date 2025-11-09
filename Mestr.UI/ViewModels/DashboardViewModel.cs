using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Mestr.UI.Command;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.Core.Model;

namespace Mestr.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IProjectService _projectService;
        private ObservableCollection<Project> _projects;

        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged(nameof(Projects));
            }
        }

        public ICommand NavigateToProjectCommand => _mainViewModel?.NavigateToProjectCommand;
        public ICommand ViewProjectDetailsCommand { get; }

        public DashboardViewModel(MainViewModel mainViewModel, IProjectService projectService)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));

            // Command that accepts a Guid parameter
            ViewProjectDetailsCommand = new RelayCommand<Guid>(ViewProjectDetails);
            
            LoadProjects();
        }

        private void LoadProjects()
        {
            var projects = _projectService.LoadAllProjects();
            Projects = new ObservableCollection<Project>(projects);
        }

        private void ViewProjectDetails(Guid projectId)
        {
            // Use MainViewModel's parameterized navigation command
            _mainViewModel.NavigateToProjectDetailsCommand.Execute(projectId);
        }
    }
}
