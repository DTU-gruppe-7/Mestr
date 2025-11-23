using Mestr.UI.View;
using Mestr.UI.Command;
using Mestr.Core.Model;
using Mestr.Services.Service;
using Mestr.Services.Interface;
using Mestr.Data.Repository;
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
        private CompanyProfile profile;

        public ViewModelBase CurrentViewModel 
        { 
            get => _currentViewModel!;
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

        public MainViewModel()
        {
            
            // Initialize services with dependencies
            _projectService = new ProjectService();
            _clientService = new ClientService();
            _earningService = new EarningService();
            _expenseService = new ExpenseService();
            _companyProfileService = new CompanyProfileService();

            // Non-parameterized navigation
            NavigateToAddNewProjectCommand = new RelayCommand(NavigateToAddNewProject);
            NavigateToDashboardCommand = new RelayCommand(NavigateToDashboard);
            NavigateToClientsCommand = new RelayCommand(NavigateToClients);

            // Parameterized navigation - expects Guid
            NavigateToProjectDetailsCommand = new RelayCommand<Guid>(NavigateToProjectDetails);
            
            // Check if company profile exists, if not show add company window
            CheckAndShowCompanyProfileWindow();
            
            // Set initial ViewModel
            CurrentViewModel = new DashboardViewModel(this, _projectService, _companyProfileService, profile);
        }

        private void CheckAndShowCompanyProfileWindow()
        {
            try
            {
                // Prøv at hente profilen fra databasen
                profile = _companyProfileService.GetProfile();

                // Hvis profilen er null (første opstart), så skal vi oprette en ny
                if (profile == null)
                {
                    // 1. Opret et nyt, tomt profil-objekt (men gem det ikke endnu)
                    profile = new CompanyProfile("", "");

                    // 2. Opret ViewModel til vinduet, og giv den den tomme profil
                    var addCompanyInfoVm = new AddCompanyInfoViewModel(_companyProfileService, profile);

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
                        this.profile = savedProfile;
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback hvis noget går helt galt
                System.Windows.MessageBox.Show("Fejl ved indlæsning af firmaprofil: " + ex.Message);
            }
        }

        private void NavigateToAddNewProject()
        {
            CurrentViewModel = new AddNewProjectViewModel(this, _projectService, _clientService);
        }

        private void NavigateToDashboard()
        {
            CurrentViewModel = new DashboardViewModel(this, _projectService, _companyProfileService, profile);
        }

        private void NavigateToClients()
        {
            CurrentViewModel = new ClientViewModel(this, _clientService);
        }

        private void NavigateToProjectDetails(Guid projectUuid)
        {
            CurrentViewModel = new ProjectDetailViewModel(this, _projectService,_earningService,_expenseService, projectUuid);
        }
    }
}
