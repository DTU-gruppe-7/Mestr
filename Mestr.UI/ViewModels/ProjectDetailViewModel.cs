using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.UI.View;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class ProjectDetailViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ProjectService _projectService;
        private readonly Guid _projectId;
        private Project _project = null!;
        private ObservableCollection<Earning> _earnings = [];
        private ObservableCollection<Expense> _expenses = [];

        public bool IsProjectCompleted => Project != null && Project.Status == ProjectStatus.Afsluttet;

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

        public ICommand NavigateToDashboardCommand => _mainViewModel.NavigateToDashboardCommand;

        public ICommand SaveProjectDetailsCommand { get; }
        public ICommand GenerateInvoiceCommand { get; }
        public ICommand ToggleProjectStatusCommand { get; }
        public ICommand ShowEconomyWindowCommand { get; }
        public ICommand DeleteProjectCommand { get; }

        public ProjectDetailViewModel(MainViewModel mainViewModel, Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;
            _projectService = new ProjectService(); // TODO: Inject this

            SaveProjectDetailsCommand = new RelayCommand(SaveProjectDetails);
            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice); 
            ShowEconomyWindowCommand = new RelayCommand(ShowEconomyWindow);
            ToggleProjectStatusCommand = new RelayCommand(ToggleProjectStatus);
            DeleteProjectCommand = new RelayCommand(DeleteProject);

            LoadProject();
        }

        private void LoadProject()
        {
            var project = _projectService.GetProjectByUuid(_projectId);
            if (project != null)
            {
                Project = project;
            }
            else
            {
                // Handle the case where the project is not found (optional: set to a default or throw)
                // Project = new Project(); // if you want to avoid nulls entirely
            }
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
            var economyWindow = new EconomyWindow
            {
                Owner = App.Current.MainWindow
            };
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

        private void ToggleProjectStatus()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            if (IsProjectCompleted)
            {
                Project.Status = ProjectStatus.Aktiv;
                _projectService.UpdateProjectStatus(_projectId, ProjectStatus.Aktiv);
            }
            else 
            {
                Project.Status = ProjectStatus.Afsluttet;
                _projectService.CompleteProject(_projectId);
            }

            OnPropertyChanged(nameof(IsProjectCompleted));
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }

        private void DeleteProject()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            var result = MessageBox.Show(
                $"Er du sikker på, at du vil slette projektet '{Project.Name}'?",
                "Bekræft sletning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _projectService.DeleteProject(_projectId);
                    _mainViewModel.NavigateToDashboardCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Kunne ikke slette projektet. Fejl: {ex.Message}",
                        "Sletning mislykkedes",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
