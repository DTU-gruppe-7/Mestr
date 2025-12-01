using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.UI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly IClientService _clientService;
        private string _projectName = string.Empty;

        public ICommand CreateProjectCommand { get; }
        public ICommand NavigateToDashboardCommand => _mainViewModel.NavigateToDashboardCommand;
        public ICommand OpenAddClientWindowCommand { get; }

        public AddNewProjectViewModel(MainViewModel mainViewModel, IProjectService projectService, IClientService clientService)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            CreateProjectCommand = new RelayCommand(CreateProject, CanCreateProject);
            OpenAddClientWindowCommand = new RelayCommand(OpenAddClientWindow);

            LoadClients();
        }

        public ObservableCollection<Client> Clients { get; } = new ObservableCollection<Client>();

        private async void LoadClients()
        {
            var clients = await _clientService.GetAllClientsAsync();
            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }
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


        private Client? _selectedClient;
        public Client? SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                ValidateSelectedClient(nameof(SelectedClient), value);
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

        private void ValidateSelectedClient(string propertyName, Client? client)
        {
            ClearErrors(propertyName);
            if (client == null)
            {
                AddError(propertyName, "Vælg venligst en kunde.");
            }
        }
        private void OpenAddClientWindow()
        {
            var addClientViewModel = new AddClientViewModel(_clientService);
            var addClientWindow = new AddClientWindow
            {
                DataContext = addClientViewModel
            };

            var result = addClientWindow.ShowDialog();

            // Reload clients after the window is closed
            LoadClients();

            // Optionally select the newly created client if any
            if (Clients.Any())
            {
                var lastClient = Clients.OrderBy(c => c.Uuid).LastOrDefault();
                SelectedClient = lastClient;
            }
        }

        private async void CreateProject()
        {
            var project = await _projectService.CreateProjectAsync(ProjectName, SelectedClient!, Description, Deadline);
            
            // Option 1: Navigate to dashboard
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
            
            // Option 2: Navigate to the newly created project details
            // _mainViewModel.NavigateToProjectDetailsCommand.Execute(project.Uuid);
        }
        private bool CanCreateProject()
        {
            return !HasErrors
                && !string.IsNullOrWhiteSpace(ProjectName)
                && SelectedClient != null;
        }

    }
}

    
