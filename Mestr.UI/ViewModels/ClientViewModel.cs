using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;

namespace Mestr.UI.ViewModels
{
    internal class ClientViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IClientService _clientService;
        private ObservableCollection<Client> _clients = [];

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged(nameof(Clients));
            }
        }

        public ICommand NavigateToDashboardCommand => _mainViewModel.NavigateToDashboardCommand;
        public ICommand NavigateToAddClientCommand { get; }
        public ICommand ViewClientDetailsCommand { get; }

        public ClientViewModel(MainViewModel mainViewModel, IClientService clientService)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));

            // Command that accepts a Guid parameter for viewing client details
            ViewClientDetailsCommand = new RelayCommand<Guid>(ViewClientDetails);
            NavigateToAddClientCommand = new RelayCommand(NavigateToAddClient);

            LoadClients();
        }

        private void LoadClients()
        {
            var clients = _clientService.GetAllClients();
            Clients = new ObservableCollection<Client>(clients);
        }

        private void NavigateToAddClient()
        {
            // Navigate to add client view (you'll need to implement this in MainViewModel)
            // _mainViewModel.NavigateToAddClientCommand.Execute(null);
            
            // For now, you can add a placeholder or implement the navigation
            // CurrentViewModel = new AddClientViewModel(this, _clientService);
        }

        private void ViewClientDetails(Guid clientId)
        {
            // Navigate to client details view (you'll need to implement this in MainViewModel)
            // _mainViewModel.NavigateToClientDetailsCommand.Execute(clientId);
        }
    }
}
