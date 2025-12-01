using Mestr.UI.View;
using Mestr.UI.Command;
using Mestr.Core.Model;
using Mestr.Services.Service;
using Mestr.Services.Interface;
using Mestr.Data.Repository;
using Mestr.UI.Utilities;
using System;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentViewModel;
        private readonly IProjectService _projectService;
        private readonly IClientService _clientService;
        private readonly IEarningService _earningService;
        private readonly IExpenseService _expenseService;
        private readonly ICompanyProfileService _companyProfileService;
        private CompanyProfile? _profile;

        public ViewModelBase? CurrentViewModel 
        { 
            get => _currentViewModel;
            set
            {
                if (_currentViewModel is IDisposable disposable)
                { 
                    disposable.Dispose();
                }
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public ICommand NavigateToAddNewProjectCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToProjectDetailsCommand { get; }
        public ICommand NavigateToClientsCommand { get; }

        public MainViewModel(
            IProjectService projectService,
            IClientService clientService,
            IEarningService earningService,
            IExpenseService expenseService,
            ICompanyProfileService companyProfileService)
        {
            
            // Initialize services with dependencies
            _projectService = projectService;
            _clientService = clientService;
            _earningService = earningService;
            _expenseService = expenseService;
            _companyProfileService = companyProfileService;

            // Non-parameterized navigation
            NavigateToAddNewProjectCommand = new RelayCommand(NavigateToAddNewProject);
            NavigateToDashboardCommand = new RelayCommand(NavigateToDashboard);
            NavigateToClientsCommand = new RelayCommand(NavigateToClients);

            // Parameterized navigation - expects Guid
            NavigateToProjectDetailsCommand = new RelayCommand<Guid>(NavigateToProjectDetails);
            
            // Check if company profile exists, if not show add company window
            CheckAndShowCompanyProfileWindow();
            
            // Set initial ViewModel - only if profile exists
            if (_profile != null)
            {
                CurrentViewModel = new DashboardViewModel(this, _projectService, _companyProfileService, _profile);
            }
            else
            {
                // If profile is still null after check, something went wrong
                MessageBoxHelper.ShowError(
                    "Firmaprofil kunne ikke indlæses. Programmet kan ikke fortsætte.",
                    "Kritisk fejl");
            }
        }

        private void CheckAndShowCompanyProfileWindow()
        {
            try
            {
                // Prøv at hente profilen fra databasen
                _profile = _companyProfileService.GetProfile();

                // Hvis profilen er null (første opstart), så skal vi oprette en ny
                if (_profile == null)
                {
                    // 1. Opret et nyt, tomt profil-objekt (men gem det ikke endnu)
                    _profile = new CompanyProfile("", "");

                    // 2. Opret ViewModel til vinduet, og giv den den tomme profil
                    var addCompanyInfoVm = new AddCompanyInfoViewModel(_companyProfileService, _profile);

                    // 3. Opret vinduet og sæt dets DataContext
                    var addCompanyWindow = new AddCompanyInfoWindow()
                    {
                        DataContext = addCompanyInfoVm,
                        // Vi undlader at sætte Owner her, da MainWindow måske ikke er helt færdig-initialiseret endnu
                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                    };

                    // 4. Vis vinduet som en dialog (blokerer indtil brugeren lukker det)
                    addCompanyWindow.ShowDialog();

                    // 5. Når vinduet lukkes: Prøv at hente profilen igen for at sikre, at vi har den gemte version
                    var savedProfile = _companyProfileService.GetProfile();
                    if (savedProfile != null)
                    {
                        _profile = savedProfile;
                    }
                    else
                    {
                        // Profile is still null - user cancelled or there was an error
                        MessageBoxHelper.ShowWarning(
                            "Firmaprofil blev ikke oprettet. Nogle funktioner vil muligvis ikke virke korrekt.",
                            "Advarsel");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Standard.LoadError($"Firmaprofil: {ex.Message}");
                _profile = null;
            }
        }

        private void NavigateToAddNewProject()
        {
            CurrentViewModel = new AddNewProjectViewModel(this, _projectService, _clientService);
        }

        private void NavigateToDashboard()
        {
            if (_profile == null)
            {
                MessageBoxHelper.ShowWarning(
                    "Firmaprofil ikke fundet. Opret venligst en firmaprofil først.",
                    "Manglende firmaprofil");
                return;
            }
            
            CurrentViewModel = new DashboardViewModel(this, _projectService, _companyProfileService, _profile);
        }

        private void NavigateToClients()
        {
            CurrentViewModel = new ClientViewModel(this, _clientService, _companyProfileService);
        }

        private void NavigateToProjectDetails(Guid projectUuid)
        {
            CurrentViewModel = new ProjectDetailViewModel(this, _projectService,_earningService,_expenseService,_companyProfileService, projectUuid);
        }
    }
}
