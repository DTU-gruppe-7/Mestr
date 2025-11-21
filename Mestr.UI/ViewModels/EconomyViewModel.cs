using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Mestr.UI.Command;

namespace Mestr.UI.ViewModels
{
    internal class EconomyViewModel : ViewModelBase
    {
        private readonly Guid _projectUuid;
        private readonly Guid? _editingId; // Null = Create mode, Value = Edit mode
        
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
        private bool _isTypeEnabled = true;

        // Constructor 1: Opret ny transaktion
        public EconomyViewModel(
            Guid projectUuid, 
            IEarningService earningService, 
            IExpenseService expenseService)
        {
            _projectUuid = projectUuid;
            _earningService = earningService ?? throw new ArgumentNullException(nameof(earningService));
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));

            TransactionTypes = new ObservableCollection<string> { "Udgift", "Indtægt" };
            Categories = new ObservableCollection<string>();
            
            // Defaults for create mode
            _selectedTransactionType = "Udgift";
            _name = string.Empty;
            _description = string.Empty;
            _selectedCategory = string.Empty;
            SelectedTransactionType = "Udgift";
            Date = DateTime.Now;
            CategoryVisibility = Visibility.Visible;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            LoadCategories();
        }

        // Constructor 2: Rediger eksisterende udgift
        public EconomyViewModel(
            Guid projectUuid, 
            IEarningService earningService, 
            IExpenseService expenseService, 
            Expense expenseToEdit) 
            : this(projectUuid, earningService, expenseService)
        {
            if (expenseToEdit == null) throw new ArgumentNullException(nameof(expenseToEdit));
            
            _editingId = expenseToEdit.Uuid;
            IsTypeEnabled = false;
            SelectedTransactionType = "Udgift";

            // Populer felter fra eksisterende data
            Name = expenseToEdit.Description;
            Description = string.Empty;
            Amount = expenseToEdit.Amount;
            Date = expenseToEdit.Date;
            IsPaid = expenseToEdit.IsAccepted;
            SelectedCategory = expenseToEdit.Category.ToString();
        }

        // Constructor 3: Rediger eksisterende indtægt
        public EconomyViewModel(
            Guid projectUuid, 
            IEarningService earningService, 
            IExpenseService expenseService, 
            Earning earningToEdit) 
            : this(projectUuid, earningService, expenseService)
        {
            if (earningToEdit == null) throw new ArgumentNullException(nameof(earningToEdit));
            
            _editingId = earningToEdit.Uuid;
            IsTypeEnabled = false;
            SelectedTransactionType = "Indtægt";

            Name = earningToEdit.Description;
            Description = string.Empty;
            Amount = earningToEdit.Amount;
            Date = earningToEdit.Date;
            IsPaid = earningToEdit.IsPaid;
        }

        // Properties
        public string WindowTitle => _editingId.HasValue 
            ? "Rediger transaktion" 
            : "Tilføj ny transaktion";

        public ObservableCollection<string> TransactionTypes { get; }
        public ObservableCollection<string> Categories { get; }

        public bool IsTypeEnabled 
        { 
            get => _isTypeEnabled; 
            set 
            { 
                _isTypeEnabled = value; 
                OnPropertyChanged(nameof(IsTypeEnabled)); 
            } 
        }

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
            get => _name; 
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
                _amount = value; 
                OnPropertyChanged(nameof(Amount));
                ValidateAmount(nameof(Amount), value);
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); 
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

        public string SelectedCategory 
        { 
            get => _selectedCategory; 
            set 
            { 
                _selectedCategory = value; 
                OnPropertyChanged(nameof(SelectedCategory)); 
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void LoadCategories()
        {
            Categories.Clear();
            if (SelectedTransactionType == "Udgift")
            {
                foreach (ExpenseCategory category in Enum.GetValues(typeof(ExpenseCategory)))
                {
                    Categories.Add(category.ToString());
                }
            }
            
            if (Categories.Count > 0 && string.IsNullOrEmpty(SelectedCategory))
            {
                SelectedCategory = Categories[0];
            }
        }

        private void UpdateCategoryVisibility()
        {
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
            try
            {
                string fullDescription = string.IsNullOrWhiteSpace(Description) 
                    ? Name 
                    : $"{Name} - {Description}";

                if (SelectedTransactionType == "Udgift")
                {
                    if (!Enum.TryParse(SelectedCategory, out ExpenseCategory categoryEnum))
                    {
                        MessageBox.Show("Ugyldig kategori valgt.", "Fejl", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (_editingId.HasValue)
                    {
                        // EDIT MODE: Hent -> Modificer -> Gem
                        var existingExpense = _expenseService.GetByUuid(_editingId.Value);
                        if (existingExpense != null)
                        {
                            existingExpense.Description = fullDescription;
                            existingExpense.Amount = Amount;
                            existingExpense.Date = Date;
                            existingExpense.Category = categoryEnum;
                            
                            _expenseService.Update(existingExpense);
                        }
                        else
                        {
                            MessageBox.Show("Udgiften blev ikke fundet.", "Fejl", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        // CREATE MODE
                        _expenseService.AddNewExpense(
                            _projectUuid, 
                            fullDescription, 
                            Amount, 
                            Date, 
                            categoryEnum);
                    }
                }
                else // Indtægt
                {
                    if (_editingId.HasValue)
                    {
                        // EDIT MODE: Hent -> Modificer -> Gem
                        var existingEarning = _earningService.GetByUuid(_editingId.Value);
                        if (existingEarning != null)
                        {
                            existingEarning.Description = fullDescription;
                            existingEarning.Amount = Amount;
                            existingEarning.Date = Date;
                            
                            _earningService.Update(existingEarning);
                        }
                        else
                        {
                            MessageBox.Show("Indtægten blev ikke fundet.", "Fejl", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        // CREATE MODE
                        _earningService.AddNewEarning(
                            _projectUuid, 
                            fullDescription, 
                            Amount, 
                            Date);
                    }
                }

                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fejl ved gemning: {ex.Message}", "Fejl", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?
                .Close();
        }
    }
}
