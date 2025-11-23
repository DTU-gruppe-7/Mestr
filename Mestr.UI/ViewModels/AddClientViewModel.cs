using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.UI.Command;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class AddClientViewModel : ViewModelBase
    {
        private readonly IClientService _clientService;
        private Client? _createdClient;

        private string _companyName = string.Empty;
        private string _contactPerson = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _address = string.Empty;
        private string _postalCode = string.Empty;
        private string _city = string.Empty;
        private string _cvr = string.Empty;

        public AddClientViewModel(IClientService clientService)
        {
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        public string WindowTitle => "Tilføj ny klient";

        public Client? CreatedClient
        {
            get => _createdClient;
            set
            {
                _createdClient = value;
                OnPropertyChanged(nameof(CreatedClient));
            }
        }

        public string CompanyName
        {
            get => _companyName;
            set
            {
                _companyName = value;
                OnPropertyChanged(nameof(CompanyName));
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string ContactPerson
        {
            get => _contactPerson;
            set
            {
                _contactPerson = value;
                OnPropertyChanged(nameof(ContactPerson));
                ValidateText(nameof(ContactPerson), value, "Kontaktperson");
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                ValidateEmail(nameof(Email), value);
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
                ValidatePhoneNumber(nameof(PhoneNumber), value);
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public string PostalCode
        {
            get => _postalCode;
            set
            {
                _postalCode = value;
                OnPropertyChanged(nameof(PostalCode));
            }
        }

        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged(nameof(City));
            }
        }

        public string CVR
        {
            get => _cvr;
            set
            {
                _cvr = value;
                OnPropertyChanged(nameof(CVR));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private bool CanSave()
        {
            return !HasErrors
                   && !string.IsNullOrWhiteSpace(ContactPerson)
                   && !string.IsNullOrWhiteSpace(Email)
                   && !string.IsNullOrWhiteSpace(PhoneNumber);
        }

        private void Save()
        {
            try
            {
                _clientService.CreateClient(
                    string.IsNullOrWhiteSpace(CompanyName) ? null : CompanyName,
                    ContactPerson,
                    Email,
                    PhoneNumber,
                    Address ?? string.Empty,
                    PostalCode ?? string.Empty,
                    City ?? string.Empty,
                    string.IsNullOrWhiteSpace(CVR) ? null : CVR
                );

                MessageBox.Show(
                    "Klienten blev oprettet succesfuldt.",
                    "Oprettelse succesfuld",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Kunne ikke oprette klienten. Fejl: {ex.Message}",
                    "Oprettelse mislykkedes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

        private void ValidateEmail(string propertyName, string email)
        {
            ClearErrors(propertyName);

            if (string.IsNullOrWhiteSpace(email))
            {
                AddError(propertyName, "Email må ikke være tom");
                return;
            }

            // Simple email validation
            if (!email.Contains("@") || !email.Contains("."))
            {
                AddError(propertyName, "Ugyldig email adresse");
            }
        }

        private void ValidatePhoneNumber(string propertyName, string phoneNumber)
        {
            ClearErrors(propertyName);

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                AddError(propertyName, "Telefonnummer må ikke være tomt");
                return;
            }

            // Check if phone number contains spaces
            if (phoneNumber.Contains(" "))
            {
                AddError(propertyName, "Telefonnummer må ikke indeholde mellemrum");
                return;
            }

            // Check if phone number starts with + (international format)
            bool isInternational = phoneNumber.StartsWith("+");

            // If international, remove the + for digit check
            string numberToCheck = isInternational ? phoneNumber[1..] : phoneNumber;

            // Check if remaining characters are only digits
            if (!numberToCheck.All(char.IsDigit))
            {
                AddError(propertyName, "Telefonnummer må kun indeholde tal og + i starten");
                return;
            }

            // Check minimum length
            if (numberToCheck.Length < 8)
            {
                AddError(propertyName, "Telefonnummer skal være mindst 8 cifre");
                return;
            }

            // Check maximum length (E.164 standard allows up to 15 digits)
            if (numberToCheck.Length > 15)
            {
                AddError(propertyName, "Telefonnummer må maksimalt være 15 cifre");
            }
        }
    }
}