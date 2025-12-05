using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.UI.Utilities;
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
        private readonly ICompanyProfileService _companyProfileService;
        private ObservableCollection<Client> _clients = [];
        private CompanyProfile? profile;

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
        public ICommand OpenCompanyInfoCommand { get; }

        public ClientViewModel(MainViewModel mainViewModel, IClientService clientService, ICompanyProfileService companyProfileService)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _companyProfileService = companyProfileService ?? throw new ArgumentNullException(nameof(companyProfileService));

            // Command that accepts a Guid parameter for viewing client details
            ViewClientDetailsCommand = new RelayCommand<Guid>(ViewClientDetails);
            NavigateToAddClientCommand = new RelayCommand(NavigateToAddClient);
            EditClientCommand = new RelayCommand<Guid>(EditClient);
            OpenCompanyInfoCommand = new RelayCommand(OpenCompanyInfo);

            LoadClients();
            profile = _companyProfileService.GetProfile();
        }

        private async void LoadClients()
        {
            var clients = await _clientService.GetAllClientsAsync();
            Clients = new ObservableCollection<Client>(clients);
        }
        private void NavigateToAddClient()
        {
            ShowAddClientWindow();
        }
        private void ShowAddClientWindow()
        {
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

        private async void EditClient(Guid clientId)
        {
            try
            {
                var client = await _clientService.GetClientByUuidAsync(clientId);
                if (client == null)
                {
                    MessageBoxHelper.Standard.ClientNotFound();
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
                MessageBoxHelper.Standard.SaveError($"Redigering af klient: {ex.Message}");
            }
        }

        private void ViewClientDetails(Guid clientId)
        {
            // Navigate to client details view (you'll need to implement this in MainViewModel)
            // _mainViewModel.NavigateToClientDetailsCommand.Execute(clientId);
        }

        private void OpenCompanyInfo()
        {
            // Hent den nyeste version fra databasen for at sikre, vi har de seneste data
            var currentProfile = _companyProfileService.GetProfile();

            if (currentProfile != null)
            {
                // Brug den hentede profil
                var addCompanyInfoVm = new AddCompanyInfoViewModel(_companyProfileService, currentProfile);

                var addCompanyInfoWindow = new AddCompanyInfoWindow()
                {
                    DataContext = addCompanyInfoVm,
                    Owner = App.Current.MainWindow,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
                };

                addCompanyInfoWindow.ShowDialog();

                // Opdater den lokale profil-variabel
                profile = _companyProfileService.GetProfile();
            }
        }
    }
}
