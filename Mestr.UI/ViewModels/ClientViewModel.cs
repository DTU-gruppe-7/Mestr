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

        public ICommand EditClientCommand { get; }

        public ClientViewModel(MainViewModel mainViewModel, IClientService clientService)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));

            // Command that accepts a Guid parameter for viewing client details
            ViewClientDetailsCommand = new RelayCommand<Guid>(ViewClientDetails);
            NavigateToAddClientCommand = new RelayCommand(NavigateToAddClient);
            EditClientCommand = new RelayCommand<Guid>(EditClient);

            LoadClients();
        }

        private void LoadClients()
        {
            var clients = _clientService.GetAllClients();
            Clients = new ObservableCollection<Client>(clients);
        }
        private void NavigateToAddClient()
        {
            ShowAddClientWindow();
        }
        private void ShowAddClientWindow()
        {
            var clientvm = new ClientViewModel(_mainViewModel, _clientService);
            var addClientVm = new AddClientViewModel(_clientService);

            var addClientWindow = new AddClientWindow()
            {
                DataContext = addClientVm,
                Owner = App.Current.MainWindow
            };

            addClientWindow.ShowDialog();

            // Reload clients after the window closes
            LoadClients();
        }

        private void EditClient(Guid clientId)
        {
            try
            {


                var client = _clientService.GetClientByUuid(clientId);
                if (client == null)
                {
                    MessageBox.Show(
                        "Klienten blev ikke fundet.",
                        "Fejl",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var editClientVm = new AddClientViewModel(_clientService, client);
                var editClientWindow = new AddClientWindow()
                {
                    DataContext = editClientVm,
                    Owner = App.Current.MainWindow
                };
                editClientWindow.ShowDialog();
                // Reload clients after the window closes
                LoadClients();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fejl ved redigering af klient: {ex.Message}",
                    "Fejl",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ViewClientDetails(Guid clientId)
        {
            // Navigate to client details view (you'll need to implement this in MainViewModel)
            // _mainViewModel.NavigateToClientDetailsCommand.Execute(clientId);
        }
    }
}
