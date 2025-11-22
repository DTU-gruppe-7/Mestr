
using Mestr.Core.Interface;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Mestr.Core.Enum;


namespace Mestr.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IProjectService _projectService;
        private ObservableCollection<Project> _projects = [];
        private ObservableCollection<Project> _completedProjects = [];
        private ObservableCollection<Project> _allOngoingProjects = [];
        
        // Filter properties
        private bool _showPlanlagt = true;
        private bool _showAktiv = true;
        private bool _showAflyst = false;
        private string _showAllButtonText = "Vis alle";

        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged(nameof(Projects));
            }
        }

        public ObservableCollection<Project> CompletedProjects
        {
            get => _completedProjects;
            set
            {
                _completedProjects = value;
                OnPropertyChanged(nameof(CompletedProjects));
            }
        }

        public string ShowAllButtonText
        {
            get => _showAllButtonText;
            set
            {
                _showAllButtonText = value;
                OnPropertyChanged(nameof(ShowAllButtonText));
            }
        }

        // Filter toggle properties
        public bool ShowPlanlagt
        {
            get => _showPlanlagt;
            set
            {
                _showPlanlagt = value;
                OnPropertyChanged(nameof(ShowPlanlagt));
                ApplyFilter();
                UpdateShowAllButtonText();
            }
        }

        public bool ShowAktiv
        {
            get => _showAktiv;
            set
            {
                _showAktiv = value;
                OnPropertyChanged(nameof(ShowAktiv));
                ApplyFilter();
                UpdateShowAllButtonText();
            }
        }

        public bool ShowAflyst
        {
            get => _showAflyst;
            set
            {
                _showAflyst = value;
                OnPropertyChanged(nameof(ShowAflyst));
                ApplyFilter();
                UpdateShowAllButtonText();
            }
        }

        public ICommand NavigateToProjectCommand => _mainViewModel.NavigateToAddNewProjectCommand;
        public ICommand NavigateToClientsCommand => _mainViewModel.NavigateToClientsCommand;
        public ICommand ViewProjectDetailsCommand { get; }
        public ICommand ShowAllCommand { get; }

        public DashboardViewModel(MainViewModel mainViewModel, IProjectService projectService)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));

            // Command that accepts a Guid parameter
            ViewProjectDetailsCommand = new RelayCommand<Guid>(ViewProjectDetails);
            ShowAllCommand = new RelayCommand(ToggleShowAll);
            
            LoadProjects();
        }

        private void LoadProjects()
        {
            var projects = _projectService.LoadOngoingProjects();
            _allOngoingProjects = new ObservableCollection<Project>(projects);
            
            var completedProjects = _projectService.LoadCompletedProjects();
            CompletedProjects = new ObservableCollection<Project>(completedProjects);
            
            ApplyFilter();
        }

        private void ToggleShowAll()
        {
            // Hvis alle er markeret, fjern alle. Ellers vis alle.
            if (AreAllFiltersSelected())
            {
                ShowPlanlagt = false;
                ShowAktiv = false;
                ShowAflyst = false;
            }
            else
            {
                ShowAktiv = true;
                ShowPlanlagt = true;
                ShowAflyst = true;
            }
        }

        private bool AreAllFiltersSelected()
        {
            return ShowPlanlagt && ShowAktiv && ShowAflyst;
        }

        private void UpdateShowAllButtonText()
        {
            ShowAllButtonText = AreAllFiltersSelected() ? "Fjern alle" : "Vis alle";
        }

        private void ApplyFilter()
        {
            var filteredProjects = _allOngoingProjects.Where(p =>
                (p.Status == ProjectStatus.Planlagt && ShowPlanlagt) ||
                (p.Status == ProjectStatus.Aktiv && ShowAktiv) ||
                (p.Status == ProjectStatus.Aflyst && ShowAflyst)
            ).ToList();

            Projects = new ObservableCollection<Project>(filteredProjects);
        }

        private void ViewProjectDetails(Guid projectId)
        {
            // Use MainViewModel's parameterized navigation command
            _mainViewModel.NavigateToProjectDetailsCommand.Execute(projectId);
        }
    }
}
