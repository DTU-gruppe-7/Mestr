using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Data.Repository;
using Mestr.Services.Service;
using Mestr.Services.Interface;
using Mestr.UI.Command;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    internal class EconomyViewModel : ViewModelBase
    {
        private readonly Guid _projectUuid;

        private readonly IEarningService _earningService;
        private readonly IExpenseService _expenseService;

        private string _selectedTransactionType;
        private string _name;
        private string _description;
        private decimal _amount;
        private string _selectedCategory;
        private DateTime _date;
        private bool _isPaid;
        private Visibility _categoryVisibility;

        public EconomyViewModel(Guid projectUuid, IEarningService earningService, IExpenseService expenseService)
        {
            _projectUuid = projectUuid;
            _earningService = earningService ?? throw new ArgumentNullException(nameof(earningService));
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
        

            // Initialize collections with Danish terms
            TransactionTypes = ["Udgift", "Indtægt"];
            Categories = [];

            // Set defaults for non-nullable fields
            _selectedTransactionType = "Udgift";
            _name = string.Empty;
            _description = string.Empty;
            _selectedCategory = string.Empty;

            SelectedTransactionType = "Udgift";
            Date = DateTime.Now;
            CategoryVisibility = Visibility.Visible; // Show category by default

            // Initialize commands
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            // Load expense categories by default
            LoadCategories();
        }

        // Collections
        public ObservableCollection<string> TransactionTypes { get; }
        public ObservableCollection<string> Categories { get; }

        // Properties
        public string SelectedTransactionType
        {
            get => _selectedTransactionType;
            set
            {
                _selectedTransactionType = value;
                OnPropertyChanged(nameof(SelectedTransactionType));
                LoadCategories();
                UpdateCategoryVisibility();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
                ValidateName(nameof(Name), value, "Navn");
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                // Fix: Remove invalid assignment and null comparison for value type
                _amount = value;
                OnPropertyChanged(nameof(Amount));
                ValidateAmount(nameof(Amount),value);
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        public bool IsPaid
        {
            get => _isPaid;
            set
            {
                _isPaid = value;
                OnPropertyChanged(nameof(IsPaid));
            }
        }

        public Visibility CategoryVisibility
        {
            get => _categoryVisibility;
            set
            {
                _categoryVisibility = value;
                OnPropertyChanged(nameof(CategoryVisibility));
            }
        }

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Methods
        private void LoadCategories()
        {
            Categories.Clear();

            if (SelectedTransactionType == "Udgift")
            {
                // Load expense categories from enum
                foreach (ExpenseCategory category in Enum.GetValues<ExpenseCategory>())
                {
                    Categories.Add(category.ToString());
                }
            }

            // Select first category by default
            if (Categories.Count > 0)
            {
                SelectedCategory = Categories[0];
            }
        }

        private void UpdateCategoryVisibility()
        {
            // Hide category when "Indtægt" is selected
            CategoryVisibility = SelectedTransactionType == "Indtægt" 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private bool CanSave()
        {
            
            return !HasErrors
                   && !string.IsNullOrWhiteSpace(Name) 
                   && Amount > 0;
        }

        private void Save()
        {

            try { 
                MessageBox.Show($"Saving {SelectedTransactionType}: {Name} - {Amount:C}", "Save Transaction");

                // Saving in the database logic 

                if (SelectedTransactionType == "Udgift")
                {
                    _expenseService.AddNewExpense(
                        _projectUuid,
                        Description,
                        Amount,
                        Date,
                        Enum.Parse<ExpenseCategory>(SelectedCategory)
                    );
                }

                else if (SelectedTransactionType == "Indtægt")
                {
                    _earningService.AddNewEarning(
                        _projectUuid,
                        Description,
                        Amount,
                        Date
                    );
                }
                // Close the window
                CloseWindow();
            }
            catch (Exception ex)
            {
                // Vis en fejlbesked, hvis noget går galt (f.eks. databaseforbindelse)
                MessageBox.Show($"Der opstod en fejl ved lagring: {ex.Message}", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow();
        }

        private static void CloseWindow()
        {
            // This will be called from the view
            Application.Current.Windows.OfType<View.EconomyWindow>().FirstOrDefault()?.Close();
        }

    }
}
