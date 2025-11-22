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

namespace Mestr.UI.ViewModels
{
    public class ProjectDetailViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ProjectService _projectService;
        private readonly PdfService _pdfService;
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

        public ProjectDetailViewModel(MainViewModel mainViewModel, Guid projectId)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _projectId = projectId;
            _projectService = new ProjectService(); // TODO: Inject this
            _pdfService = new PdfService();

            SaveProjectDetailsCommand = new RelayCommand(SaveProjectDetails);
            GenerateInvoiceCommand = new RelayCommand(GenerateInvoice);

            ShowEconomyWindowCommand = new RelayCommand(ShowEconomyWindow);

            ToggleProjectStatusCommand = new RelayCommand(ToggleProjectStatus);

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

        private async void GenerateInvoice()
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
                // PDF-generering og filskrivning på baggrundstråd
                var pdfBytes = await Task.Run(() => _pdfService.GeneratePdfInvoice(Project));
                await Task.Run(() => File.WriteAllBytes(filePath, pdfBytes));

                // Åbn PDF på UI-tråden
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
                Project.Status = ProjectStatus.Igangværende;
                _projectService.UpdateProjectStatus(_projectId, ProjectStatus.Igangværende);
            }
            else 
            {
                Project.Status = ProjectStatus.Igangværende;
                _projectService.CompleteProject(_projectId);
            }

            OnPropertyChanged(nameof(IsProjectCompleted));
            _mainViewModel.NavigateToDashboardCommand.Execute(null);
        }
    }
}
