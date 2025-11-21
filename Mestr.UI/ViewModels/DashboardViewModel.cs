using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Mestr.UI.Command;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.Core.Model;
using System.Linq;

namespace Mestr.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IProjectService _projectService;
        private ObservableCollection<Project> _projects = [];
        private ObservableCollection<Project> _completedProjects = [];

        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged(nameof(Projects));
            }
        }

        public ObservableCollection<Project>CompletedProjects
        {
            get => _completedProjects;
            set
            {
                _completedProjects = value;
                OnPropertyChanged(nameof(CompletedProjects));
            }
        }

        public ICommand NavigateToProjectCommand => _mainViewModel.NavigateToProjectCommand;
        public ICommand ViewProjectDetailsCommand { get; }

        public DashboardViewModel(MainViewModel mainViewModel, IProjectService projectService)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));

            // Command that accepts a Guid parameter
            ViewProjectDetailsCommand = new RelayCommand<Guid>(ViewProjectDetails);
            
            LoadProjects();
        }

        private void ViewProjectDetails(Guid projectId)
        {
            // Use MainViewModel's parameterized navigation command
            _mainViewModel.NavigateToProjectDetailsCommand.Execute(projectId);
        }

        private int _sortIndex;
        public int SortIndex
        {
            get => _sortIndex;
            set
            {
                if (_sortIndex != value)
                {
                    _sortIndex = value;
                    OnPropertyChanged(nameof(SortIndex));
                    ApplySorting();
                }
            }
        }

        private void ApplySorting()
        {
            // Sort ongoing projects
            if (Projects != null && Projects.Count > 0)
            {
                IEnumerable<Project> sorted = Projects;
                switch (SortIndex)
                {
                    case 0: // alfabetisk
                        sorted = Projects.OrderBy(p => p.Name);
                        break;
                    case 1: // tidligst først
                        sorted = Projects.OrderBy(p => p.EndDate);
                        break;
                    case 2: // senest først
                        sorted = Projects.OrderByDescending(p => p.EndDate);
                        break;
                }
                var sortedList = sorted.ToList();
                Projects.Clear();
                foreach (var p in sortedList)
                    Projects.Add(p);
            }
            // Sort completed projects
            if (CompletedProjects != null && CompletedProjects.Count > 0)
            {
                IEnumerable<Project> sortedCompleted = CompletedProjects;
                switch (SortIndex)
                {
                    case 0: // alfabetisk
                        sortedCompleted = CompletedProjects.OrderBy(p => p.Name);
                        break;
                    case 1: // tidligst først
                        sortedCompleted = CompletedProjects.OrderBy(p => p.EndDate);
                        break;
                    case 2: // senest først
                        sortedCompleted = CompletedProjects.OrderByDescending(p => p.EndDate);
                        break;
                }
                var sortedCompletedList = sortedCompleted.ToList();
                CompletedProjects.Clear();
                foreach (var p in sortedCompletedList)
                    CompletedProjects.Add(p);
            }
        }

        private void LoadProjects()
        {
            var projects = _projectService.LoadOngoingProjects();
            Projects = new ObservableCollection<Project>(projects);

            var completedProjects = _projectService.LoadCompletedProjects();
            CompletedProjects = new ObservableCollection<Project>(completedProjects);
        }
    }
}
