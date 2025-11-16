using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public bool HasErrors => _errors.Count > 0;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private readonly Dictionary<string, List<string>> _errors = new();
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Error handling
        protected void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        protected void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
        public IEnumerable GetErrors(string propertyName)
        {
            return propertyName != null && _errors.ContainsKey(propertyName)
                ? _errors[propertyName]
                : null;
        }

        protected void AmountBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow digits and one decimal separator
            Regex regex = new Regex(@"^[0-9]*(\.[0-9]*)?$");
            TextBox textBox = sender as TextBox;

            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !regex.IsMatch(newText);
        }

        // Validation logic
        protected void ValidateName(string propertyName, string value, string displayName)
        {
            ClearErrors(propertyName);
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(propertyName, $"{displayName} kan ikke være tomt.");
            }
            else if (value.Length < 3)
            {
                AddError(propertyName, $"{displayName} skal være mindst 3 tegn langt");
            }
        }

        protected void ValidateAmount(decimal amount)
        {
            ClearErrors(nameof(amount));

            // Check if amount is zero or negative
            if (amount <= 0)
            {
                AddError(nameof(amount), "Beløb skal være større end 0.");
            }

            // Optional: Check for upper limit
            if (amount > 1_000_000) // Example max limit
            {
                AddError(nameof(amount), "Beløb må ikke overstige 1.000.000.");
            }
        }

        protected void ValidateDate(string propertyName, DateTime? date)
        {
            ClearErrors(propertyName);

            if (date.HasValue && date.Value < DateTime.Now)
            {
                AddError(propertyName, "Deadline skal være en fremtidig dato.");
            }
        }
    }
}