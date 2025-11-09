using System;
using System.Windows.Input;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.Core.Model;
using Mestr.UI.Command;

namespace Mestr.UI.ViewModels
{
    public class ProjectDetailViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IProjectService _projectService;
        private readonly Guid _projectId;
        private Project _project;

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
        public ICommand CompleteProjectCommand { get; }


        public ProjectDetailViewModel(MainViewModel mainViewModel, Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;
            _projectService = new ProjectService(); // TODO: Inject this

            CompleteProjectCommand = new RelayCommand(CompleteProject);
            LoadProject();
        }

        private void LoadProject()
        {
            Project = _projectService.GetProjectByUuid(_projectId);
        }

        private void CompleteProject()
        {
            if (Project != null)
            {
                _projectService.CompleteProject(_projectId);
                _mainViewModel.NavigateToDashboardCommand.Execute(null);
            }
        }
    }
}
