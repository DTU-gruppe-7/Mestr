using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Command;
using Mestr.UI.View;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class ProjectDetailViewModel : ViewModelBase
    {
        private readonly EarningService _earningService;
        private readonly ExpenseService _expenseService;
        private readonly MainViewModel _mainViewModel;
        private readonly ProjectService _projectService;
        private readonly PdfService _pdfService;
        private readonly Guid _projectId;
        private Project _project = null!;
        private ObservableCollection<Earning> _earnings;
        private ObservableCollection<Expense> _expenses;

        public bool IsProjectCompleted => Project != null && Project.Status == ProjectStatus.Afsluttet;

        public ObservableCollection<Earning> Earnings
        {
            get => _earnings;   
            set
            {
                _earnings = value;
                OnPropertyChanged(nameof(Earnings));
                OnPropertyChanged(nameof(ProfitLoss));
                OnPropertyChanged(nameof(ProfitLossColor));
            }
        }

        public ObservableCollection<Expense> Expenses
        {
            get => _expenses;
            set
            {
                _expenses = value;
                OnPropertyChanged(nameof(Expenses));
                OnPropertyChanged(nameof(ProfitLoss));
                OnPropertyChanged(nameof(ProfitLossColor));
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

        
        public ICommand EditEarningCommand { get; }
        public ICommand EditExpenseCommand { get; }

        public ICommand DeleteProjectCommand { get; }


        public ProjectDetailViewModel(MainViewModel mainViewModel, Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;
            _pdfService = new PdfService();
            _projectService = new ProjectService();
            _earningService = new EarningService();
            _expenseService = new ExpenseService();

            SaveProjectDetailsCommand = new RelayCommand(SaveProjectDetails);

            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice);
            ShowEconomyWindowCommand = new RelayCommand(ShowEconomyWindow);
            ToggleProjectStatusCommand = new RelayCommand(ToggleProjectStatus);
            EditEarningCommand = new RelayCommand<Earning>(EditEarning);
            EditExpenseCommand = new RelayCommand<Expense>(EditExpense);
            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice); 
            ShowEconomyWindowCommand = new RelayCommand(ShowEconomyWindow);
            ToggleProjectStatusCommand = new RelayCommand(ToggleProjectStatus);
            DeleteProjectCommand = new RelayCommand(DeleteProject);

            LoadProject();
        }

        private void LoadProject()
        {
            var project = _projectService.GetProjectByUuid(_projectId);
            var earningsList = _earningService.GetAllByProjectUuid(_projectId);
            var expensesList = _expenseService.GetAllByProjectUuid(_projectId);

            if (project != null)
            {
                Project = project;
                Earnings = new ObservableCollection<Earning>(earningsList);
                Expenses = new ObservableCollection<Expense>(expensesList);
            }
        }

        private void SaveProjectDetails()
        {
            if (Project == null || Project.Uuid == Guid.Empty) return;

            try
            {
                _projectService.UpdateProject(Project);
                MessageBox.Show(
                    "Projektet blev gemt succesfuldt.",
                    "Gem succesfuldt",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                    _mainViewModel.NavigateToDashboardCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Kunne ikke gemme projektet. Fejl: {ex.Message}",
                    "Gem mislykkedes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void GenerateInvoice()
        {
            if (Project == null)
                return;

            var dialog = new SaveFileDialog
            {
                FileName = $"Invoice_{Project.Name}.pdf",
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            string filePath = dialog.FileName;

            try
            {
                // Generer PDF som byte-array
                var pdfBytes = _pdfService.GeneratePdfInvoice(Project);

                // Gem filen synkront
                File.WriteAllBytes(filePath, pdfBytes);

                // �bn filen
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fejl ved generering af PDF: {ex.Message}");
            }
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }




        private void ShowEconomyWindow()
        {
            var economyVm = new EconomyViewModel(
                _projectId,
                _earningService,
                _expenseService);

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            LoadProject();
        }

        private void EditEarning(Earning? earning)
        {
            if (earning == null) return;

            var economyVm = new EconomyViewModel(
                _projectId,
                _earningService,
                _expenseService,
                earning); // Pass earning to edit constructor

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            LoadProject(); // Refresh data efter lukket dialog
        }

        private void EditExpense(Expense? expense)
        {
            if (expense == null) return;

            var economyVm = new EconomyViewModel(
                _projectId,
                _earningService,
                _expenseService,
                expense); // Pass expense to edit constructor

            var economyWindow = new EconomyWindow()
            {
                DataContext = economyVm,
                Owner = App.Current.MainWindow
            };

            economyWindow.ShowDialog();
            LoadProject(); // Refresh data after closing dialog
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


        public decimal ProfitLoss
        {
            get
            {
                decimal totalIncome = 0;
                decimal totalExpense = 0;

                if (Earnings != null)
                    totalIncome = Earnings.Sum(e => e.Amount);

                if (Expenses != null)
                    totalExpense = Expenses.Sum(e => e.Amount);

                return totalIncome - totalExpense;
             }
          }

        public string ProfitLossColor
        {
            get
            {
                var profitLoss = ProfitLoss;
                if (profitLoss < 0)
                    return "#E02020"; // Rød
                else if (profitLoss > 0)
                    return "#008800"; // Grøn
                else
                    return "#666666"; // Grå/neutral for 0
            }
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
