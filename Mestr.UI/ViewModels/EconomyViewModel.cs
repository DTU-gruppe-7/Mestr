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
        private readonly Guid? _editingId;
        
        private readonly IEarningService _earningService;
        private readonly IExpenseService _expenseService;
        private readonly Action<Earning>? _onEarningSaved;
        private readonly Action<Expense>? _onExpenseSaved;
        private readonly Action<Earning>? _onEarningDeleted;
        private readonly Action<Expense>? _onExpenseDeleted;

        private string _selectedTransactionType;
        private string _description;
        private decimal _amount;
        private string _selectedCategory;
        private DateTime _date;
        private bool _isPaid;
        private Visibility _categoryVisibility;
        private bool _isTypeEnabled = true;

        // Constructor 1: Opret ny transaktion (accepterer services som parameter)
        public EconomyViewModel(
            Guid projectUuid, 
            IEarningService earningService, 
            IExpenseService expenseService,
            Action<Earning>? onEarningSaved = null,
            Action<Expense>? onExpenseSaved = null)
        {
            _projectUuid = projectUuid;
            _earningService = earningService ?? throw new ArgumentNullException(nameof(earningService));
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _onEarningSaved = onEarningSaved;
            _onExpenseSaved = onExpenseSaved;

            TransactionTypes = new ObservableCollection<string> { "Udgift", "Indtægt" };
            Categories = new ObservableCollection<string>();
            
            // Defaults for create mode
            _selectedTransactionType = "Udgift";
            _description = string.Empty;
            _selectedCategory = string.Empty;
            SelectedTransactionType = "Udgift";
            Date = DateTime.Now;
            CategoryVisibility = Visibility.Visible;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
            LoadCategories();
        }

        // Constructor 2: Rediger eksisterende udgift
        public EconomyViewModel(
            Guid projectUuid, 
            IEarningService earningService, 
            IExpenseService expenseService, 
            Expense expenseToEdit,
            Action<Expense>? onExpenseSaved = null,
            Action<Expense>? onExpenseDeleted = null) 
            : this(projectUuid, earningService, expenseService, null, onExpenseSaved)
        {
            if (expenseToEdit == null) throw new ArgumentNullException(nameof(expenseToEdit));
            
            _editingId = expenseToEdit.Uuid;
            _onExpenseDeleted = onExpenseDeleted;
            IsTypeEnabled = false;
            SelectedTransactionType = "Udgift";

            Description = expenseToEdit.Description;
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
            Earning earningToEdit,
            Action<Earning>? onEarningSaved = null,
            Action<Earning>? onEarningDeleted = null) 
            : this(projectUuid, earningService, expenseService, onEarningSaved, null)
        {
            if (earningToEdit == null) throw new ArgumentNullException(nameof(earningToEdit));
            
            _editingId = earningToEdit.Uuid;
            _onEarningDeleted = onEarningDeleted;
            IsTypeEnabled = false;
            SelectedTransactionType = "Indtægt";
            Description = earningToEdit.Description;
            Amount = earningToEdit.Amount;
            Date = earningToEdit.Date;
            IsPaid = earningToEdit.IsPaid;
        }

        // Properties
        public string WindowTitle => _editingId.HasValue 
            ? "Rediger transaktion" 
            : "Tilføj ny transaktion";

        public Visibility DeleteButtonVisibility => _editingId.HasValue 
            ? Visibility.Visible 
            : Visibility.Collapsed;

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

        public string Description 
        { 
            get => _description; 
            set 
            { 
                _description = value; 
                OnPropertyChanged(nameof(Description));
                ValidateText(nameof(Description), value, "Beskrivelse");
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
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
        public ICommand DeleteCommand { get; }

        private void LoadCategories()
        {
            Categories.Clear();
            if (SelectedTransactionType == "Udgift")
            {
                foreach (ExpenseCategory category in Enum.GetValues<ExpenseCategory>())
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
                   && !string.IsNullOrWhiteSpace(Description) 
                   && Amount > 0;
        }

        private void Save()
        {
            try
            {
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
                        // EDIT MODE: Use the existing object passed in, don't fetch from database
                        // Create a new expense object with the updated values
                        var updatedExpense = new Expense(_editingId.Value, Description, Amount, Date, categoryEnum, IsPaid)
                        {
                            ProjectUuid = _projectUuid
                        };
                        
                        // Notify parent through callback
                        _onExpenseSaved?.Invoke(updatedExpense);
                    }
                    else
                    {
                        // CREATE MODE: Create new object
                        var newExpense = new Expense(Guid.NewGuid(), Description, Amount, Date, categoryEnum, IsPaid)
                        {
                            ProjectUuid = _projectUuid
                        };
                        
                        // Notify parent through callback
                        _onExpenseSaved?.Invoke(newExpense);
                    }
                }
                else // Indtægt
                {
                    if (_editingId.HasValue)
                    {
                        // EDIT MODE: Use the existing object passed in, don't fetch from database
                        // Create a new earning object with the updated values
                        var updatedEarning = new Earning(_editingId.Value, Description, Amount, Date, IsPaid)
                        {
                            ProjectUuid = _projectUuid
                        };
                        
                        // Notify parent through callback
                        _onEarningSaved?.Invoke(updatedEarning);
                    }
                    else
                    {
                        // CREATE MODE: Create new object
                        var newEarning = new Earning(Guid.NewGuid(), Description, Amount, Date, IsPaid)
                        {
                            ProjectUuid = _projectUuid
                        };
                        
                        // Notify parent through callback
                        _onEarningSaved?.Invoke(newEarning);
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

        private bool CanDelete()
        {
            return _editingId.HasValue;
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                $"Er du sikker på, at du vil slette denne {(SelectedTransactionType == "Udgift" ? "udgift" : "indtægt")}?",
                "Bekræft sletning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                if (SelectedTransactionType == "Udgift")
                {
                    // Create expense object with current values for deletion
                    if (!Enum.TryParse(SelectedCategory, out ExpenseCategory categoryEnum))
                    {
                        categoryEnum = ExpenseCategory.Andet;
                    }
                    
                    var expense = new Expense(_editingId!.Value, Description, Amount, Date, categoryEnum, IsPaid)
                    {
                        ProjectUuid = _projectUuid
                    };
                    
                    // Notify parent through callback
                    _onExpenseDeleted?.Invoke(expense);
                }
                else // Indtægt
                {
                    // Create earning object with current values for deletion
                    var earning = new Earning(_editingId!.Value, Description, Amount, Date, IsPaid)
                    {
                        ProjectUuid = _projectUuid
                    };
                    
                    // Notify parent through callback
                    _onEarningDeleted?.Invoke(earning);
                }

                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fejl ved sletning: {ex.Message}", "Fejl",
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
