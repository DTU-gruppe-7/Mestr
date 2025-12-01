using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Mestr.Core.Constants;

namespace Mestr.UI.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public bool HasErrors => _errors.Count > 0;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private readonly Dictionary<string, List<string>> _errors = [];
        
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Error handling
        protected void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = [];

            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        protected void ClearErrors(string propertyName)
        {
            if (!_errors.ContainsKey(propertyName))
            {
                return;
            }
            _errors.Remove(propertyName);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        
        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName != null && _errors.TryGetValue(propertyName, out var errors))
                return errors;
            return Enumerable.Empty<string>();
        }

        // Validation logic
        protected void ValidateText(string propertyName, string value, string displayName)
        {
            ClearErrors(propertyName);
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(propertyName, $"{displayName} kan ikke være tomt.");
            }
            else if (value.Length < AppConstants.Validation.MinTextLength)
            {
                AddError(propertyName, 
                    string.Format(AppConstants.ErrorMessages.DescriptionTooShort, AppConstants.Validation.MinTextLength));
            }
        }

        protected void ValidateAmount(string propertyName, decimal amount)
        {
            ClearErrors(propertyName);

            // Check if amount is zero or negative
            if (amount <= 0)
            {
                AddError(propertyName, AppConstants.ErrorMessages.AmountMustBePositive);
            }

            // Check for upper limit
            if (amount > AppConstants.Validation.MaxAmount)
            {
                AddError(propertyName, 
                    string.Format(AppConstants.ErrorMessages.AmountTooLarge, AppConstants.Validation.MaxAmount));
            }
        }

        protected void ValidateDate(string propertyName, DateTime? date)
        {
            ClearErrors(propertyName);

            if (date.HasValue && date.Value.Date < DateTime.Today)
            {
                AddError(propertyName, AppConstants.ErrorMessages.DateMustBeFuture);
            }
        }
    }
}